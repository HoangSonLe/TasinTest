using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.AuthorPredicates;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebServices
{
    public class PurchaseOrderService : BaseService<PurchaseOrderService>, IPurchaseOrderService
    {
        private readonly IMapper _mapper;
        private IPurchaseOrderRepository _purchaseOrderRepository;
        private IPurchaseOrderItemRepository _purchaseOrderItemRepository;
        private ICustomerRepository _customerRepository;
        private IProductRepository _productRepository;
        private IUnitRepository _unitRepository;
        private IProcessingTypeRepository _processingTypeRepository;

        public PurchaseOrderService(
            ILogger<PurchaseOrderService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserContext currentUserContext,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            SampleDBContext dbContext,
            IMapper mapper,
            IPurchaseOrderRepository purchaseOrderRepository,
            IPurchaseOrderItemRepository purchaseOrderItemRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IUnitRepository unitRepository,
            IProcessingTypeRepository processingTypeRepository) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _unitRepository = unitRepository;
            _processingTypeRepository = processingTypeRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<PurchaseOrderViewModel>>>> GetPurchaseOrderList(PurchaseOrderSearchModel searchModel)
        {
            var ack = new Acknowledgement<JsonResultPaging<List<PurchaseOrderViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Purchase_Order>(true);

                // Only get active purchase orders
                predicate = predicate.And(p => p.IsActive);

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(searchModel.SearchString))
                {
                    var searchValue = searchModel.SearchString.Trim().ToLower();
                    predicate = predicate.And(p => p.Code.ToLower().Contains(searchValue));
                }

                if (searchModel.Customer_ID.HasValue)
                {
                    predicate = predicate.And(p => p.Customer_ID == searchModel.Customer_ID.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchModel.Status))
                {
                    predicate = predicate.And(p => p.Status == searchModel.Status);
                }

                // Add author predicate
                predicate = PurchaseOrderAuthorPredicate.GetPurchaseOrderAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var purchaseOrderQuery = await _purchaseOrderRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var purchaseOrderViewModels = _mapper.Map<List<PurchaseOrderViewModel>>(purchaseOrderQuery.Data);

                // Get customer names
                var customerIds = purchaseOrderViewModels.Select(p => p.Customer_ID).Distinct().ToList();
                var customers = await _customerRepository.ReadOnlyRespository.GetAsync(
                    filter: c => customerIds.Contains(c.ID)
                );

                // Populate customer names
                foreach (var purchaseOrder in purchaseOrderViewModels)
                {
                    var customer = customers.FirstOrDefault(c => c.ID == purchaseOrder.Customer_ID);
                    if (customer != null)
                    {
                        purchaseOrder.CustomerName = customer.Name;
                    }
                }

                var result = new JsonResultPaging<List<PurchaseOrderViewModel>>
                {
                    Data = purchaseOrderViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = purchaseOrderQuery.TotalRecords
                };

                ack.IsSuccess = true;
                ack.Data = result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPurchaseOrderList: {ex.Message}");
                ack.AddMessage(ex.Message);
            }

            return ack;
        }

        public async Task<Acknowledgement<PurchaseOrderViewModel>> GetPurchaseOrderById(int purchaseOrderId)
        {
            var ack = new Acknowledgement<PurchaseOrderViewModel>();
            try
            {
                var purchaseOrder = await _purchaseOrderRepository.ReadOnlyRespository.FindAsync(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    ack.AddMessage("Không tìm thấy đơn hàng.");
                    return ack;
                }

                // Check author predicate
                var predicate = PredicateBuilder.New<Purchase_Order>(true);
                predicate = predicate.And(p => p.ID == purchaseOrderId);
                predicate = PurchaseOrderAuthorPredicate.GetPurchaseOrderAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var authorizedPurchaseOrders = await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                    filter: predicate
                );

                if (authorizedPurchaseOrders.Count == 0)
                {
                    ack.AddMessage("Bạn không có quyền xem đơn hàng này.");
                    return ack;
                }

                var purchaseOrderViewModel = _mapper.Map<PurchaseOrderViewModel>(purchaseOrder);

                // Get customer name
                var customer = await _customerRepository.ReadOnlyRespository.FindAsync(purchaseOrder.Customer_ID);
                if (customer != null)
                {
                    purchaseOrderViewModel.CustomerName = customer.Name;
                }

                // Get purchase order items
                var purchaseOrderItems = await _purchaseOrderItemRepository.GetByPurchaseOrderIdAsync(purchaseOrderId);

                var purchaseOrderItemViewModels = _mapper.Map<List<PurchaseOrderItemViewModel>>(purchaseOrderItems);

                // Get product, unit, and processing type names
                var productIds = purchaseOrderItemViewModels.Select(p => p.Product_ID).Distinct().ToList();
                var unitIds = purchaseOrderItemViewModels.Where(p => p.Unit_ID.HasValue).Select(p => p.Unit_ID.Value).Distinct().ToList();
                var processingTypeIds = purchaseOrderItemViewModels.Where(p => p.ProcessingType_ID.HasValue).Select(p => p.ProcessingType_ID.Value).Distinct().ToList();

                var products = await _productRepository.ReadOnlyRespository.GetAsync(
                    filter: p => productIds.Contains(p.ID)
                );

                var units = await _unitRepository.ReadOnlyRespository.GetAsync(
                    filter: u => unitIds.Contains(u.ID)
                );

                var processingTypes = await _processingTypeRepository.ReadOnlyRespository.GetAsync(
                    filter: pt => processingTypeIds.Contains(pt.ID)
                );

                // Populate names
                foreach (var item in purchaseOrderItemViewModels)
                {
                    var product = products.FirstOrDefault(p => p.ID == item.Product_ID);
                    if (product != null)
                    {
                        item.ProductName = product.Name;
                    }

                    if (item.Unit_ID.HasValue)
                    {
                        var unit = units.FirstOrDefault(u => u.ID == item.Unit_ID.Value);
                        if (unit != null)
                        {
                            item.UnitName = unit.Name;
                        }
                    }

                    if (item.ProcessingType_ID.HasValue)
                    {
                        var processingType = processingTypes.FirstOrDefault(pt => pt.ID == item.ProcessingType_ID.Value);
                        if (processingType != null)
                        {
                            item.ProcessingTypeName = processingType.Name;
                        }
                    }
                }

                purchaseOrderViewModel.PurchaseOrderItems = purchaseOrderItemViewModels;

                ack.IsSuccess = true;
                ack.Data = purchaseOrderViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPurchaseOrderById: {ex.Message}");
                ack.AddMessage(ex.Message);
            }

            return ack;
        }

        public async Task<Acknowledgement> CreateOrUpdatePurchaseOrder(PurchaseOrderViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (postData.Id == 0)
                {
                    // Create new purchase order
                    var newPurchaseOrder = _mapper.Map<Purchase_Order>(postData);
                    newPurchaseOrder.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.PurchaseOrder, DbContext);
                    newPurchaseOrder.CreatedDate = DateTime.Now;
                    newPurchaseOrder.CreatedBy = CurrentUserId;
                    newPurchaseOrder.UpdatedDate = newPurchaseOrder.CreatedDate;
                    newPurchaseOrder.UpdatedBy = newPurchaseOrder.CreatedBy;

                    // Calculate totals
                    CalculateTotals(newPurchaseOrder, postData.PurchaseOrderItems);

                    await ack.TrySaveChangesAsync(res => res.AddAsync(newPurchaseOrder), _purchaseOrderRepository.Repository);

                    if (ack.IsSuccess && postData.PurchaseOrderItems.Any())
                    {
                        // Save purchase order items
                        await SavePurchaseOrderItems(newPurchaseOrder.ID, postData.PurchaseOrderItems);
                    }
                }
                else
                {
                    // Update existing purchase order
                    var existingPurchaseOrder = await _purchaseOrderRepository.Repository.FindAsync(postData.Id);
                    if (existingPurchaseOrder == null)
                    {
                        ack.AddMessage("Không tìm thấy đơn hàng.");
                        return ack;
                    }

                    // Check author predicate
                    var predicate = PredicateBuilder.New<Purchase_Order>(true);
                    predicate = predicate.And(p => p.ID == postData.Id);
                    predicate = PurchaseOrderAuthorPredicate.GetPurchaseOrderAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                    var authorizedPurchaseOrders = await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                        filter: predicate
                    );

                    if (authorizedPurchaseOrders.Count == 0)
                    {
                        ack.AddMessage("Bạn không có quyền cập nhật đơn hàng này.");
                        return ack;
                    }

                    existingPurchaseOrder.Customer_ID = postData.Customer_ID;
                    existingPurchaseOrder.Status = postData.Status;
                    existingPurchaseOrder.UpdatedDate = DateTime.Now;
                    existingPurchaseOrder.UpdatedBy = CurrentUserId;

                    // Calculate totals
                    CalculateTotals(existingPurchaseOrder, postData.PurchaseOrderItems);

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingPurchaseOrder), _purchaseOrderRepository.Repository);

                    if (ack.IsSuccess && postData.PurchaseOrderItems.Any())
                    {
                        // Delete existing items
                        await _purchaseOrderItemRepository.DeleteByPurchaseOrderIdAsync(postData.Id);

                        // Save new items
                        await SavePurchaseOrderItems(existingPurchaseOrder.ID, postData.PurchaseOrderItems);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateOrUpdatePurchaseOrder: {ex.Message}");
                ack.AddMessage(ex.Message);
            }

            return ack;
        }

        private void CalculateTotals(Purchase_Order purchaseOrder, List<PurchaseOrderItemViewModel> items)
        {
            decimal totalPrice = 0;
            decimal totalPriceNoTax = 0;

            foreach (var item in items)
            {
                if (item.Price.HasValue)
                {
                    decimal itemTotal = item.Quantity * item.Price.Value;
                    totalPriceNoTax += itemTotal;

                    if (item.TaxRate.HasValue)
                    {
                        decimal taxAmount = itemTotal * (item.TaxRate.Value / 100);
                        totalPrice += itemTotal + taxAmount;
                    }
                    else
                    {
                        totalPrice += itemTotal;
                    }
                }
            }

            purchaseOrder.TotalPrice = totalPrice;
            purchaseOrder.TotalPriceNoTax = totalPriceNoTax;
        }

        private async Task SavePurchaseOrderItems(int purchaseOrderId, List<PurchaseOrderItemViewModel> items)
        {
            foreach (var item in items)
            {
                var purchaseOrderItem = _mapper.Map<Purchase_Order_Item>(item);
                purchaseOrderItem.PO_ID = purchaseOrderId;

                await _purchaseOrderItemRepository.Repository.AddAsync(purchaseOrderItem);
            }

            await DbContext.SaveChangesAsync();
        }

        public async Task<Acknowledgement> DeletePurchaseOrderById(int purchaseOrderId)
        {
            var ack = new Acknowledgement();
            try
            {
                var purchaseOrder = await _purchaseOrderRepository.Repository.FindAsync(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    ack.AddMessage("Không tìm thấy đơn hàng.");
                    return ack;
                }

                // Check author predicate
                var predicate = PredicateBuilder.New<Purchase_Order>(true);
                predicate = predicate.And(p => p.ID == purchaseOrderId);
                predicate = PurchaseOrderAuthorPredicate.GetPurchaseOrderAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var authorizedPurchaseOrders = await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                    filter: predicate
                );

                if (authorizedPurchaseOrders.Count == 0)
                {
                    ack.AddMessage("Bạn không có quyền xóa đơn hàng này.");
                    return ack;
                }

                if(purchaseOrder.Status != ((int)EPOStatus.New).ToString())
                {
                    ack.AddMessage("Đơn hàng không thể xóa (Chỉ có thể xóa đơn hàng trạng thái mới).");
                    return ack;
                }

                // Set purchase order as inactive instead of deleting
                purchaseOrder.IsActive = false;
                purchaseOrder.UpdatedDate = DateTime.Now;
                purchaseOrder.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(purchaseOrder), _purchaseOrderRepository.Repository);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeletePurchaseOrderById: {ex.Message}");
                ack.AddMessage(ex.Message);
            }

            return ack;
        }

        public new void Dispose()
        {
            // Call base class Dispose which handles common resources
            base.Dispose();

            // No need to dispose repositories as they are managed by DI container
        }
    }
}
