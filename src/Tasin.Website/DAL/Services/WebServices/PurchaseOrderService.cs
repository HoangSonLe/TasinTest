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
            IUnitRepository unitRepository) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _unitRepository = unitRepository;
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

                // Get purchase order items for all purchase orders
                var purchaseOrderIds = purchaseOrderViewModels.Select(p => p.Id).ToList();
                var allPurchaseOrderItems = await _purchaseOrderItemRepository.ReadOnlyRespository.GetAsync(
                    filter: poi => purchaseOrderIds.Contains(poi.PO_ID)
                );

                var purchaseOrderItemViewModels = _mapper.Map<List<PurchaseOrderItemViewModel>>(allPurchaseOrderItems);

                // Get product and unit names for all items (only if there are items)
                List<Product> products = new List<Product>();
                List<Unit> units = new List<Unit>();

                if (purchaseOrderItemViewModels.Any())
                {
                    var productIds = purchaseOrderItemViewModels.Select(p => p.Product_ID).Distinct().ToList();
                    var unitIds = purchaseOrderItemViewModels.Where(p => p.Unit_ID.HasValue).Select(p => p.Unit_ID.Value).Distinct().ToList();

                    if (productIds.Any())
                    {
                        products = await _productRepository.ReadOnlyRespository.GetAsync(
                            filter: p => productIds.Contains(p.ID)
                        );
                    }

                    if (unitIds.Any())
                    {
                        units = await _unitRepository.ReadOnlyRespository.GetAsync(
                            filter: u => unitIds.Contains(u.ID)
                        );
                    }
                }

                // Populate names for purchase order items
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

                    // ProcessingTypeName is now computed from enum in ViewModel
                }

                // Populate customer names and purchase order items
                foreach (var purchaseOrder in purchaseOrderViewModels)
                {
                    var customer = customers.FirstOrDefault(c => c.ID == purchaseOrder.Customer_ID);
                    if (customer != null)
                    {
                        purchaseOrder.CustomerName = customer.Name;
                    }

                    // Assign purchase order items to each purchase order
                    purchaseOrder.PurchaseOrderItems = purchaseOrderItemViewModels
                        .Where(poi => poi.PO_ID == purchaseOrder.Id)
                        .ToList();
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

                // Get product and unit names
                var productIds = purchaseOrderItemViewModels.Select(p => p.Product_ID).Distinct().ToList();
                var unitIds = purchaseOrderItemViewModels.Where(p => p.Unit_ID.HasValue).Select(p => p.Unit_ID.Value).Distinct().ToList();

                var products = await _productRepository.ReadOnlyRespository.GetAsync(
                    filter: p => productIds.Contains(p.ID)
                );

                var units = await _unitRepository.ReadOnlyRespository.GetAsync(
                    filter: u => unitIds.Contains(u.ID)
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

                    // ProcessingTypeName is now computed from enum in ViewModel
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

            // Input validation
            if (postData == null)
            {
                ack.AddMessage("Dữ liệu đơn hàng không hợp lệ.");
                return ack;
            }

            if (postData.Customer_ID <= 0)
            {
                ack.AddMessage("Vui lòng chọn khách hàng.");
                return ack;
            }

            if (postData.PurchaseOrderItems == null || !postData.PurchaseOrderItems.Any())
            {
                ack.AddMessage("Đơn hàng phải có ít nhất một sản phẩm.");
                return ack;
            }

            // Validate purchase order items
            foreach (var item in postData.PurchaseOrderItems)
            {
                if (item.Product_ID <= 0)
                {
                    ack.AddMessage("Sản phẩm không hợp lệ.");
                    return ack;
                }

                if (item.Quantity <= 0)
                {
                    ack.AddMessage("Số lượng sản phẩm phải lớn hơn 0.");
                    return ack;
                }
            }

            // Use transaction to ensure data consistency
            using var transaction = await DbContext.Database.BeginTransactionAsync();
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

                    await _purchaseOrderRepository.Repository.AddWithoutSaveAsync(newPurchaseOrder);
                    await DbContext.SaveChangesAsync();

                    if (postData.PurchaseOrderItems.Count > 0)
                    {
                        await SavePurchaseOrderItemsInTransaction(newPurchaseOrder.ID, postData.PurchaseOrderItems);
                        await DbContext.SaveChangesAsync();
                    }

                    ack.IsSuccess = true;
                }
                else
                {
                    // Update existing purchase order
                    var existingPurchaseOrder = await _purchaseOrderRepository.Repository.FindAsync(postData.Id);
                    if (existingPurchaseOrder == null)
                    {
                        ack.AddMessage("Không tìm thấy đơn hàng.");
                        await transaction.RollbackAsync();
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
                        await transaction.RollbackAsync();
                        return ack;
                    }

                    // Allow editing for New status only
                    if (existingPurchaseOrder.Status != EPOStatus.New.ToString())
                    {
                        if (existingPurchaseOrder.Status == EPOStatus.Executed.ToString())
                        {
                            ack.AddMessage("Không thể chỉnh sửa đơn hàng đã được tạo đơn tổng hợp.");
                        }
                        else if (existingPurchaseOrder.Status == EPOStatus.Cancel.ToString())
                        {
                            ack.AddMessage("Không thể chỉnh sửa đơn hàng đã bị hủy.");
                        }
                        else
                        {
                            ack.AddMessage("Đơn hàng không thể chỉnh sửa (Chỉ có thể chỉnh sửa đơn hàng ở trạng thái: Mới).");
                        }
                        await transaction.RollbackAsync();
                        return ack;
                    }

                    existingPurchaseOrder.Customer_ID = postData.Customer_ID;
                    existingPurchaseOrder.Status = postData.Status.ToString();
                    existingPurchaseOrder.UpdatedDate = DateTime.Now;
                    existingPurchaseOrder.UpdatedBy = CurrentUserId;

                    // Calculate totals
                    CalculateTotals(existingPurchaseOrder, postData.PurchaseOrderItems);

                    // Update purchase order (without saving)
                    _purchaseOrderRepository.Repository.UpdateWithoutSave(existingPurchaseOrder);

                    if (postData.PurchaseOrderItems.Count > 0)
                    {
                        await DeletePurchaseOrderItemsInTransaction(postData.Id);
                        await SavePurchaseOrderItemsInTransaction(existingPurchaseOrder.ID, postData.PurchaseOrderItems);
                    }

                    // Save all changes within transaction
                    await DbContext.SaveChangesAsync();
                    ack.IsSuccess = true;
                }

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateOrUpdatePurchaseOrder failed: {ErrorMessage}", ex.Message);
                ack.AddMessage(ex.Message);
                ack.IsSuccess = false;

                // Rollback transaction on error
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError("Transaction rollback failed: {ErrorMessage}", rollbackEx.Message);
                }
            }

            return ack;
        }

        private static void CalculateTotals(Purchase_Order purchaseOrder, List<PurchaseOrderItemViewModel> items)
        {
            decimal totalPrice = 0;
            decimal totalPriceNoTax = 0;

            foreach (var item in items)
            {
                if (item.Price.HasValue)
                {
                    decimal baseAmount = item.Quantity * item.Price.Value;
                    // Removed lossAmount from calculation as per requirement
                    decimal totalProcessingFee = item.Quantity * (item.ProcessingFee ?? 0);
                    decimal totalBeforeTax = baseAmount + (item.AdditionalCost ?? 0) + totalProcessingFee;
                    decimal taxAmount = totalBeforeTax * ((item.TaxRate ?? 0) / 100);

                    totalPriceNoTax += totalBeforeTax;
                    totalPrice += totalBeforeTax + taxAmount;
                }
            }

            purchaseOrder.TotalPrice = totalPrice;
            purchaseOrder.TotalPriceNoTax = totalPriceNoTax;
        }

        /// <summary>
        /// Save purchase order items within a transaction (does not call SaveChanges)
        /// </summary>
        private async Task SavePurchaseOrderItemsInTransaction(int purchaseOrderId, List<PurchaseOrderItemViewModel> items)
        {
            if (items == null || items.Count == 0)
                return;

            var purchaseOrderItems = new List<Purchase_Order_Item>();

            foreach (var item in items)
            {
                var purchaseOrderItem = new Purchase_Order_Item
                {
                    PO_ID = purchaseOrderId,
                    Product_ID = item.Product_ID,
                    Quantity = item.Quantity,
                    Unit_ID = item.Unit_ID,
                    Price = item.Price,
                    TaxRate = item.TaxRate,
                    LossRate = item.LossRate,
                    AdditionalCost = item.AdditionalCost,
                    ProcessingFee = item.ProcessingFee,
                    Note = item.Note
                };

                purchaseOrderItems.Add(purchaseOrderItem);
            }

            // Add items to context without saving (transaction will handle the save)
            await _purchaseOrderItemRepository.Repository.AddRangeWithoutSaveAsync(purchaseOrderItems);
        }

        /// <summary>
        /// Delete purchase order items within a transaction (does not call SaveChanges)
        /// </summary>
        private async Task DeletePurchaseOrderItemsInTransaction(int purchaseOrderId)
        {
            var existingItems = await _purchaseOrderItemRepository.ReadOnlyRespository.GetAsync(
                filter: item => item.PO_ID == purchaseOrderId
            );

            if (existingItems.Count > 0)
            {
                _purchaseOrderItemRepository.Repository.DeleteRangeWithoutSave(existingItems);
            }
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

                // Allow deletion for New status only
                if (purchaseOrder.Status != EPOStatus.New.ToString())
                {
                    if (purchaseOrder.Status == EPOStatus.Executed.ToString())
                    {
                        ack.AddMessage("Không thể xóa đơn hàng đã được tạo đơn tổng hợp.");
                    }
                    else if (purchaseOrder.Status == EPOStatus.Cancel.ToString())
                    {
                        ack.AddMessage("Không thể xóa đơn hàng đã bị hủy.");
                    }
                    else
                    {
                        ack.AddMessage("Đơn hàng không thể xóa (Chỉ có thể xóa đơn hàng ở trạng thái: Mới).");
                    }
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
                _logger.LogError("DeletePurchaseOrderById failed: {ErrorMessage}", ex.Message);
                ack.AddMessage(ex.Message);
            }

            return ack;
        }

        public async Task<Acknowledgement> CancelPurchaseOrderById(int purchaseOrderId)
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
                    ack.AddMessage("Bạn không có quyền hủy đơn hàng này.");
                    return ack;
                }

                // Check if order can be cancelled - only allow cancellation for New and Confirmed statuses
                var allowedCancelStatuses = new[] {
                    EPOStatus.New.ToString(),
                    EPOStatus.Confirmed.ToString()
                };

                if (!allowedCancelStatuses.Contains(purchaseOrder.Status))
                {
                    if (purchaseOrder.Status == EPOStatus.Cancel.ToString())
                    {
                        ack.AddMessage("Đơn hàng đã được hủy trước đó.");
                    }
                    else if (purchaseOrder.Status == EPOStatus.Executed.ToString())
                    {
                        ack.AddMessage("Không thể hủy đơn hàng đã được tạo đơn tổng hợp.");
                    }
                    else
                    {
                        ack.AddMessage("Đơn hàng không thể hủy (Chỉ có thể hủy đơn hàng ở trạng thái: Mới, Đã xác nhận).");
                    }
                    return ack;
                }

                // Update status to Cancel
                purchaseOrder.Status = EPOStatus.Cancel.ToString();
                purchaseOrder.UpdatedDate = DateTime.Now;
                purchaseOrder.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(purchaseOrder), _purchaseOrderRepository.Repository);
            }
            catch (Exception ex)
            {
                _logger.LogError("CancelPurchaseOrderById failed: {ErrorMessage}", ex.Message);
                ack.AddMessage(ex.Message);
            }

            return ack;
        }

        public new void Dispose()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
