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
    public class PurchaseAgreementService : BaseService<PurchaseAgreementService>, IPurchaseAgreementService
    {
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private IPurchaseAgreementRepository _purchaseAgreementRepository;
        private IPurchaseAgreementItemRepository _purchaseAgreementItemRepository;
        private IPurchaseOrderRepository _purchaseOrderRepository;
        private IPurchaseOrderItemRepository _purchaseOrderItemRepository;
        private IVendorRepository _vendorRepository;
        private IProductRepository _productRepository;
        private IUnitRepository _unitRepository;
        private IProduct_VendorRepository _productVendorRepository;

        public PurchaseAgreementService(
            ILogger<PurchaseAgreementService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserContext currentUserContext,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            SampleDBContext dbContext,
            IMapper mapper,
            ICommonService commonService,
            IPurchaseAgreementRepository purchaseAgreementRepository,
            IPurchaseAgreementItemRepository purchaseAgreementItemRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            IPurchaseOrderItemRepository purchaseOrderItemRepository,
            IVendorRepository vendorRepository,
            IProductRepository productRepository,
            IUnitRepository unitRepository,
            IProduct_VendorRepository productVendorRepository) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _commonService = commonService;
            _purchaseAgreementRepository = purchaseAgreementRepository;
            _purchaseAgreementItemRepository = purchaseAgreementItemRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
            _vendorRepository = vendorRepository;
            _productRepository = productRepository;
            _unitRepository = unitRepository;
            _productVendorRepository = productVendorRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<PurchaseAgreementViewModel>>>> GetPurchaseAgreementList(PurchaseAgreementSearchModel searchModel)
        {
            var ack = new Acknowledgement<JsonResultPaging<List<PurchaseAgreementViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Purchase_Agreement>(true);

                // Only get active purchase agreements
                predicate = predicate.And(p => p.IsActive);

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(searchModel.SearchString))
                {
                    var searchValue = searchModel.SearchString.Trim().ToLower();
                    predicate = predicate.And(p => p.Code.ToLower().Contains(searchValue) ||
                                                   p.GroupCode.ToLower().Contains(searchValue));
                }

                if (searchModel.Vendor_ID.HasValue)
                {
                    predicate = predicate.And(p => p.Vendor_ID == searchModel.Vendor_ID.Value);
                }

                if (searchModel.Status.HasValue)
                {
                    predicate = predicate.And(p => p.Status == searchModel.Status.Value.ToString());
                }

                // Apply author predicate
                predicate = PurchaseAgreementAuthorPredicate.GetPurchaseAgreementAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var purchaseAgreementQuery = await _purchaseAgreementRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(p => p.CreatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var purchaseAgreementViewModels = _mapper.Map<List<PurchaseAgreementViewModel>>(purchaseAgreementQuery.Data);

                // Get vendor names
                var vendorIds = purchaseAgreementViewModels.Select(p => p.Vendor_ID).Distinct().ToList();
                var vendors = await _vendorRepository.ReadOnlyRespository.GetAsync(
                    filter: v => vendorIds.Contains(v.ID)
                );

                // Populate vendor names
                foreach (var purchaseAgreement in purchaseAgreementViewModels)
                {
                    var vendor = vendors.FirstOrDefault(v => v.ID == purchaseAgreement.Vendor_ID);
                    if (vendor != null)
                    {
                        purchaseAgreement.VendorName = vendor.Name;
                    }
                }

                // Get created by names
                var createdByIds = purchaseAgreementViewModels.Select(p => p.CreatedBy).Distinct().ToList();
                var users = await _userRepository.ReadOnlyRespository.GetAsync(
                    filter: u => createdByIds.Contains(u.Id)
                );

                foreach (var purchaseAgreement in purchaseAgreementViewModels)
                {
                    var user = users.FirstOrDefault(u => u.Id == purchaseAgreement.CreatedBy);
                    if (user != null)
                    {
                        purchaseAgreement.UpdatedByName = user.Name;
                    }
                }

                ack.Data = new JsonResultPaging<List<PurchaseAgreementViewModel>>
                {
                    Data = purchaseAgreementViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = purchaseAgreementQuery.TotalRecords
                };
                ack.IsSuccess = true;
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi lấy danh sách đơn tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error getting purchase agreement list");
            }
            return ack;
        }

        public async Task<Acknowledgement<PurchaseAgreementViewModel>> GetPurchaseAgreementById(int purchaseAgreementId)
        {
            var ack = new Acknowledgement<PurchaseAgreementViewModel>();
            try
            {
                var purchaseAgreement = await _purchaseAgreementRepository.ReadOnlyRespository.FindAsync(purchaseAgreementId);
                if (purchaseAgreement == null)
                {
                    ack.AddMessage("Không tìm thấy đơn tổng hợp.");
                    return ack;
                }

                // Check author predicate
                var predicate = PredicateBuilder.New<Purchase_Agreement>(true);
                predicate = predicate.And(p => p.ID == purchaseAgreementId);
                predicate = PurchaseAgreementAuthorPredicate.GetPurchaseAgreementAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var authorizedPurchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(
                    filter: predicate
                );

                if (!authorizedPurchaseAgreements.Any())
                {
                    ack.AddMessage("Bạn không có quyền xem đơn tổng hợp này.");
                    return ack;
                }

                var purchaseAgreementViewModel = _mapper.Map<PurchaseAgreementViewModel>(purchaseAgreement);

                // Get vendor name
                var vendor = await _vendorRepository.ReadOnlyRespository.FindAsync(purchaseAgreement.Vendor_ID);
                if (vendor != null)
                {
                    purchaseAgreementViewModel.VendorName = vendor.Name;
                }

                // Get purchase agreement items
                var purchaseAgreementItems = await _purchaseAgreementItemRepository.GetByPurchaseAgreementIdAsync(purchaseAgreementId);

                var purchaseAgreementItemViewModels = _mapper.Map<List<PurchaseAgreementItemViewModel>>(purchaseAgreementItems);

                // Get product and unit names
                var productIds = purchaseAgreementItemViewModels.Select(p => p.Product_ID).Distinct().ToList();
                var unitIds = purchaseAgreementItemViewModels.Where(p => p.Unit_ID.HasValue).Select(p => p.Unit_ID.Value).Distinct().ToList();

                var products = await _productRepository.ReadOnlyRespository.GetAsync(
                    filter: p => productIds.Contains(p.ID)
                );

                var units = await _unitRepository.ReadOnlyRespository.GetAsync(
                    filter: u => unitIds.Contains(u.ID)
                );

                // Populate product and unit names
                foreach (var item in purchaseAgreementItemViewModels)
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
                }

                purchaseAgreementViewModel.PurchaseAgreementItems = purchaseAgreementItemViewModels;

                // Get created by name
                var createdByUser = await _userRepository.ReadOnlyRespository.FindAsync(purchaseAgreement.CreatedBy);
                if (createdByUser != null)
                {
                    purchaseAgreementViewModel.UpdatedByName = createdByUser.Name;
                }

                ack.Data = purchaseAgreementViewModel;
                ack.IsSuccess = true;
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi lấy thông tin đơn tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error getting purchase agreement by id");
            }
            return ack;
        }

        public async Task<Acknowledgement> UpdatePurchaseAgreement(PurchaseAgreementViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (postData.Id == 0)
                {
                    // Create new purchase agreement
                    var newPurchaseAgreement = _mapper.Map<Purchase_Agreement>(postData);
                    newPurchaseAgreement.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.PurchaseAgreement, DbContext);
                    newPurchaseAgreement.CreatedDate = DateTime.Now;
                    newPurchaseAgreement.CreatedBy = CurrentUserId;
                    newPurchaseAgreement.UpdatedDate = newPurchaseAgreement.CreatedDate;
                    newPurchaseAgreement.UpdatedBy = newPurchaseAgreement.CreatedBy;

                    // Calculate total price
                    newPurchaseAgreement.TotalPrice = postData.PurchaseAgreementItems.Sum(item => (item.Price ?? 0) * item.Quantity);

                    await ack.TrySaveChangesAsync(res => res.AddAsync(newPurchaseAgreement), _purchaseAgreementRepository.Repository);

                    if (ack.IsSuccess && postData.PurchaseAgreementItems.Any())
                    {
                        await SavePurchaseAgreementItems(newPurchaseAgreement.ID, postData.PurchaseAgreementItems);
                    }
                }
                else
                {
                    // Update existing purchase agreement
                    var existingPurchaseAgreement = await _purchaseAgreementRepository.ReadOnlyRespository.FindAsync(postData.Id);
                    if (existingPurchaseAgreement == null)
                    {
                        ack.AddMessage("Không tìm thấy đơn tổng hợp để cập nhật.");
                        return ack;
                    }

                    // Check author predicate
                    var predicate = PredicateBuilder.New<Purchase_Agreement>(true);
                    predicate = predicate.And(p => p.ID == postData.Id);
                    predicate = PurchaseAgreementAuthorPredicate.GetPurchaseAgreementAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                    var authorizedPurchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(
                        filter: predicate
                    );

                    if (!authorizedPurchaseAgreements.Any())
                    {
                        ack.AddMessage("Bạn không có quyền cập nhật đơn tổng hợp này.");
                        return ack;
                    }

                    existingPurchaseAgreement.Vendor_ID = postData.Vendor_ID;
                    existingPurchaseAgreement.GroupCode = postData.GroupCode;
                    existingPurchaseAgreement.Note = postData.Note;
                    existingPurchaseAgreement.Status = postData.Status.ToString();
                    existingPurchaseAgreement.UpdatedDate = DateTime.Now;
                    existingPurchaseAgreement.UpdatedBy = CurrentUserId;

                    // Calculate total price
                    existingPurchaseAgreement.TotalPrice = postData.PurchaseAgreementItems.Sum(item => (item.Price ?? 0) * item.Quantity);

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingPurchaseAgreement), _purchaseAgreementRepository.Repository);

                    if (ack.IsSuccess && postData.PurchaseAgreementItems.Any())
                    {
                        // Delete existing items
                        await _purchaseAgreementItemRepository.DeleteByPurchaseAgreementIdAsync(postData.Id);

                        // Save new items
                        await SavePurchaseAgreementItems(existingPurchaseAgreement.ID, postData.PurchaseAgreementItems);
                    }
                }
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi lưu đơn tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error creating or updating purchase agreement");
            }
            return ack;
        }

        public async Task<Acknowledgement> DeletePurchaseAgreementById(int purchaseAgreementId)
        {
            var ack = new Acknowledgement();
            try
            {
                var purchaseAgreement = await _purchaseAgreementRepository.ReadOnlyRespository.FindAsync(purchaseAgreementId);
                if (purchaseAgreement == null)
                {
                    ack.AddMessage("Không tìm thấy đơn tổng hợp để xóa.");
                    return ack;
                }

                // Check author predicate
                var predicate = PredicateBuilder.New<Purchase_Agreement>(true);
                predicate = predicate.And(p => p.ID == purchaseAgreementId);
                predicate = PurchaseAgreementAuthorPredicate.GetPurchaseAgreementAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var authorizedPurchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(
                    filter: predicate
                );

                if (!authorizedPurchaseAgreements.Any())
                {
                    ack.AddMessage("Bạn không có quyền xóa đơn tổng hợp này.");
                    return ack;
                }

                // Soft delete by setting IsActive to false
                purchaseAgreement.IsActive = false;
                purchaseAgreement.UpdatedDate = DateTime.Now;
                purchaseAgreement.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(purchaseAgreement), _purchaseAgreementRepository.Repository);
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi xóa đơn tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error deleting purchase agreement");
            }
            return ack;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<PAGroupViewModel>>>> GetPAGroupList(PAGroupSearchModel searchModel)
        {
            var ack = new Acknowledgement<JsonResultPaging<List<PAGroupViewModel>>>();
            try
            {
                // Get all purchase agreements grouped by GroupCode
                var predicate = PredicateBuilder.New<Purchase_Agreement>(true);
                predicate = predicate.And(p => p.IsActive);

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(searchModel.SearchString))
                {
                    var searchValue = searchModel.SearchString.Trim().ToLower();
                    predicate = predicate.And(p => p.GroupCode.ToLower().Contains(searchValue));
                }

                if (searchModel.Status.HasValue)
                {
                    predicate = predicate.And(p => p.Status == searchModel.Status.Value.ToString());
                }

                if (!string.IsNullOrWhiteSpace(searchModel.GroupCode))
                {
                    predicate = predicate.And(p => p.GroupCode == searchModel.GroupCode);
                }

                if (searchModel.Vendor_ID.HasValue)
                {
                    predicate = predicate.And(p => p.Vendor_ID == searchModel.Vendor_ID.Value);
                }

                if (searchModel.DateFrom.HasValue)
                {
                    predicate = predicate.And(p => p.CreatedDate >= searchModel.DateFrom.Value);
                }

                if (searchModel.DateTo.HasValue)
                {
                    predicate = predicate.And(p => p.CreatedDate <= searchModel.DateTo.Value);
                }

                // Apply author predicate
                predicate = PurchaseAgreementAuthorPredicate.GetPurchaseAgreementAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var purchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(p => p.CreatedDate)
                );

                // Group by GroupCode to create PA ViewModels
                var groupedPAs = purchaseAgreements.GroupBy(pa => pa.GroupCode).ToList();

                var paViewModels = new List<PAGroupViewModel>();
                foreach (var group in groupedPAs)
                {
                    var childPAs = _mapper.Map<List<PurchaseAgreementViewModel>>(group.ToList());

                    // Get vendor names for child PAs
                    var vendorIds = childPAs.Select(p => p.Vendor_ID).Distinct().ToList();
                    var vendors = await _vendorRepository.ReadOnlyRespository.GetAsync(
                        filter: v => vendorIds.Contains(v.ID)
                    );

                    foreach (var childPA in childPAs)
                    {
                        var vendor = vendors.FirstOrDefault(v => v.ID == childPA.Vendor_ID);
                        if (vendor != null)
                        {
                            childPA.VendorName = vendor.Name;
                        }
                    }

                    var paViewModel = new PAGroupViewModel
                    {
                        GroupCode = group.Key,
                        TotalPrice = group.Sum(pa => pa.TotalPrice),
                        Status = Enum.Parse<EPAStatus>(group.First().Status),
                        CreatedDate = group.Min(pa => pa.CreatedDate),
                        CreatedBy = group.First().CreatedBy,
                        UpdatedDate = group.Max(pa => pa.UpdatedDate),
                        UpdatedBy = group.First().UpdatedBy,
                        ChildPAs = childPAs
                    };

                    paViewModels.Add(paViewModel);
                }

                // Apply pagination to grouped results
                var totalRecords = paViewModels.Count;
                var pagedResults = paViewModels
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .ToList();

                ack.Data = new JsonResultPaging<List<PAGroupViewModel>>
                {
                    Data = pagedResults,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = totalRecords
                };
                ack.IsSuccess = true;
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi lấy danh sách PA tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error getting PA list");
            }
            return ack;
        }

        public async Task<Acknowledgement<PAGroupViewModel>> GetPAByGroupCode(string groupCode)
        {
            var ack = new Acknowledgement<PAGroupViewModel>();
            try
            {
                var purchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(
                    filter: pa => pa.GroupCode == groupCode && pa.IsActive,
                    orderBy: q => q.OrderBy(pa => pa.Vendor_ID)
                );

                if (!purchaseAgreements.Any())
                {
                    ack.AddMessage("Không tìm thấy PA tổng hợp với GroupCode này.");
                    return ack;
                }

                // Check author predicate
                var predicate = PredicateBuilder.New<Purchase_Agreement>(true);
                predicate = predicate.And(p => p.GroupCode == groupCode);
                predicate = PurchaseAgreementAuthorPredicate.GetPurchaseAgreementAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var authorizedPAs = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(filter: predicate);
                if (!authorizedPAs.Any())
                {
                    ack.AddMessage("Bạn không có quyền xem PA tổng hợp này.");
                    return ack;
                }

                var childPAs = _mapper.Map<List<PurchaseAgreementViewModel>>(purchaseAgreements);

                // Get vendor names and items for each child PA
                foreach (var childPA in childPAs)
                {
                    var vendor = await _vendorRepository.ReadOnlyRespository.FindAsync(childPA.Vendor_ID);
                    if (vendor != null)
                    {
                        childPA.VendorName = vendor.Name;
                    }

                    // Get items for this PA
                    var items = await _purchaseAgreementItemRepository.GetByPurchaseAgreementIdAsync(childPA.Id);
                    childPA.PurchaseAgreementItems = _mapper.Map<List<PurchaseAgreementItemViewModel>>(items);

                    // Get product and unit names for items
                    if (childPA.PurchaseAgreementItems != null)
                    {
                        var productIds = childPA.PurchaseAgreementItems.Select(i => i.Product_ID).Distinct().ToList();
                        var unitIds = childPA.PurchaseAgreementItems.Where(i => i.Unit_ID.HasValue).Select(i => i.Unit_ID.Value).Distinct().ToList();

                        var products = await _productRepository.ReadOnlyRespository.GetAsync(filter: p => productIds.Contains(p.ID));
                        var units = await _unitRepository.ReadOnlyRespository.GetAsync(filter: u => unitIds.Contains(u.ID));

                        foreach (var item in childPA.PurchaseAgreementItems)
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
                        }
                    }

                    var paViewModel = new PAGroupViewModel
                    {
                        GroupCode = groupCode,
                        TotalPrice = purchaseAgreements.Sum(pa => pa.TotalPrice),
                        Status = Enum.Parse<EPAStatus>(purchaseAgreements.First().Status),
                        CreatedDate = purchaseAgreements.Min(pa => pa.CreatedDate),
                        CreatedBy = purchaseAgreements.First().CreatedBy,
                        UpdatedDate = purchaseAgreements.Max(pa => pa.UpdatedDate),
                        UpdatedBy = purchaseAgreements.First().UpdatedBy,
                        ChildPAs = childPAs
                    };

                    ack.Data = paViewModel;
                    ack.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi lấy thông tin PA tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error getting PA by group code");
            }
            return ack;
        }

        public async Task<Acknowledgement<PAGroupViewModel>> GetPAGroupPreview()
        {
            var ack = new Acknowledgement<PAGroupViewModel>();
            try
            {
                // Get all confirmed purchase orders with items in one query
                var confirmedOrders = await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                    filter: po => po.Status == EPOStatus.Confirmed.ToString() && po.IsActive,
                    includeProperties: "PurchaseOrderItems"
                );

                if (!confirmedOrders.Any())
                {
                    ack.AddMessage("Không có đơn hàng nào ở trạng thái Đã xác nhận để tạo PA tổng hợp.");
                    return ack;
                }

                // Use included items instead of separate query
                var allOrderItems = confirmedOrders.SelectMany(po => po.PurchaseOrderItems).ToList();

                if (!allOrderItems.Any())
                {
                    ack.AddMessage("Không có sản phẩm nào trong các đơn hàng đã xác nhận.");
                    return ack;
                }

                // Get product information for transformation
                var productIds = allOrderItems.Select(poi => poi.Product_ID).Distinct().ToList();
                var products = await _productRepository.ReadOnlyRespository.GetAsync(
                    filter: p => productIds.Contains(p.ID)
                );
                var productLookup = products.ToDictionary(p => p.ID, p => p);

                // Get parent products for child products that have ParentID
                var childProducts = products.Where(p => p.ParentID.HasValue).ToList();
                var parentProductIds = childProducts.Select(p => p.ParentID!.Value).Distinct().ToList();

                var parentProducts = new List<Product>();
                if (parentProductIds.Any())
                {
                    parentProducts = (await _productRepository.ReadOnlyRespository.GetAsync(
                        filter: p => parentProductIds.Contains(p.ID)
                    )).ToList();

                    // Add parent products to lookup if not already present
                    foreach (var parentProduct in parentProducts)
                    {
                        if (!productLookup.ContainsKey(parentProduct.ID))
                        {
                            productLookup[parentProduct.ID] = parentProduct;
                        }
                    }
                }

                // Transform child products to parent products and calculate quantities with loss rate
                var transformedOrderItems = allOrderItems.Select(item =>
                {
                    var product = productLookup.TryGetValue(item.Product_ID, out var prod) ? prod : null;

                    // If product has a parent, use parent product instead
                    var targetProductId = product?.ParentID ?? item.Product_ID;

                    // Calculate quantity with loss rate for all products that have LossRate
                    var adjustedQuantity = item.Quantity;
                    if (product?.LossRate.HasValue == true)
                    {
                        // Apply loss rate: (100 + LossRate) * Quantity / 100
                        adjustedQuantity = (100 + product.LossRate.Value) * item.Quantity / 100;
                    }

                    return new
                    {
                        OriginalProductId = item.Product_ID,
                        TargetProductId = targetProductId,
                        Quantity = adjustedQuantity,
                        Price = item.Price,
                        Unit_ID = item.Unit_ID,
                        TaxRate = item.TaxRate,
                        ProcessingFee = item.ProcessingFee,
                        POItemId = item.ID
                    };
                }).ToList();

                // Get all target product IDs for vendor relationships
                var allTargetProductIds = transformedOrderItems.Select(i => i.TargetProductId).Distinct().ToList();

                // Get product-vendor relationships for all target products with highest priority vendors
                var productVendors = await _productVendorRepository.GetHighestPriorityVendorsByProductIdsAsync(allTargetProductIds);

                // Create lookup dictionary for better performance - each product maps to its highest priority vendor
                var productVendorLookup = productVendors.ToDictionary(pv => pv.Product_ID, pv => pv.Vendor_ID);

                // Create product-vendor price lookup for getting vendor prices
                var productVendorPriceLookup = productVendors
                    .ToDictionary(pv => $"{pv.Product_ID}_{pv.Vendor_ID}", pv => pv.UnitPrice ?? 0);

                // Group transformed items by vendor using lookup
                var vendorGroups = transformedOrderItems
                    .Where(item => productVendorLookup.ContainsKey(item.TargetProductId))
                    .GroupBy(item => productVendorLookup[item.TargetProductId])
                    .ToDictionary(g => g.Key, g => g.ToList());

                if (!vendorGroups.Any())
                {
                    ack.AddMessage("Không tìm thấy thông tin nhà cung cấp cho các sản phẩm trong đơn hàng.");
                    return ack;
                }

                // Get all vendor information in one query
                var vendorIds = vendorGroups.Keys.ToList();
                var vendors = await _vendorRepository.ReadOnlyRespository.GetAsync(
                    filter: v => vendorIds.Contains(v.ID)
                );
                var vendorLookup = vendors.ToDictionary(v => v.ID, v => v);

                // Get all unit information for display
                var unitIds = transformedOrderItems.Where(item => item.Unit_ID.HasValue).Select(item => item.Unit_ID.Value).Distinct().ToList();

                var units = await _unitRepository.ReadOnlyRespository.GetAsync(
                    filter: u => unitIds.Contains(u.ID)
                );

                var unitLookup = units.ToDictionary(u => u.ID, u => u);

                var previewChildPAs = new List<PurchaseAgreementViewModel>();

                foreach (var vendorGroup in vendorGroups)
                {
                    var vendorId = vendorGroup.Key;
                    var vendorItems = vendorGroup.Value;

                    if (!vendorLookup.TryGetValue(vendorId, out var vendor))
                        continue;

                    // Group by target product and sum quantities
                    var productGroups = vendorItems.GroupBy(item => item.TargetProductId)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            TotalQuantity = g.Sum(i => i.Quantity),
                            UnitId = g.First().Unit_ID
                        }).ToList();

                    // Create preview PA items using target products with vendor prices
                    var paItems = productGroups.Select(productGroup =>
                    {
                        // Get vendor price from product-vendor relationship
                        var vendorPrice = productVendorPriceLookup.TryGetValue($"{productGroup.ProductId}_{vendorId}", out var price) ? price : 0;

                        return new PurchaseAgreementItemViewModel
                        {
                            Product_ID = productGroup.ProductId,
                            ProductName = productLookup.TryGetValue(productGroup.ProductId, out var product) ? product.Name : "",
                            Quantity = productGroup.TotalQuantity,
                            Unit_ID = productGroup.UnitId,
                            UnitName = productGroup.UnitId.HasValue && unitLookup.TryGetValue(productGroup.UnitId.Value, out var unit) ? unit.Name : "",
                            Price = vendorPrice // Use vendor price
                        };
                    }).ToList();

                    // Calculate total price using vendor prices
                    var totalPrice = paItems.Sum(item => item.Quantity * item.Price);

                    var previewPA = new PurchaseAgreementViewModel
                    {
                        Code = $"[Preview] PA-{vendor.Name}",
                        Vendor_ID = vendorId,
                        VendorName = vendor.Name,
                        TotalPrice = totalPrice ?? 0,
                        Status = EPAStatus.New,
                        PurchaseAgreementItems = paItems
                    };

                    previewChildPAs.Add(previewPA);
                }

                // Create preview PA Group ViewModel
                var previewPAGroup = new PAGroupViewModel
                {
                    GroupCode = "[Preview] Sẽ được tạo tự động",
                    TotalPrice = previewChildPAs.Sum(pa => pa.TotalPrice),
                    Status = EPAStatus.New,
                    CreatedDate = DateTime.Now,
                    ChildPAs = previewChildPAs
                };

                ack.Data = previewPAGroup;
                ack.IsSuccess = true;
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi lấy preview PA tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error getting PA group preview");
            }
            return ack;
        }

        public async Task<Acknowledgement<PAGroupViewModel>> CreatePAGroup()
        {
            var ack = new Acknowledgement<PAGroupViewModel>();

            // Use database transaction to ensure data consistency
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                // Get all confirmed purchase orders with items in one query
                var confirmedOrders = await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                    filter: po => po.Status == EPOStatus.Confirmed.ToString() && po.IsActive,
                    includeProperties: "PurchaseOrderItems"
                );

                if (!confirmedOrders.Any())
                {
                    ack.AddMessage("Không có đơn hàng nào ở trạng thái Đã xác nhận để tạo PA tổng hợp.");
                    return ack;
                }

                // Use included items instead of separate query
                var allOrderItems = confirmedOrders.SelectMany(po => po.PurchaseOrderItems).ToList();

                if (!allOrderItems.Any())
                {
                    ack.AddMessage("Không có sản phẩm nào trong các đơn hàng đã xác nhận.");
                    return ack;
                }

                // Get product information for transformation
                var productIds = allOrderItems.Select(poi => poi.Product_ID).Distinct().ToList();
                var products = await _productRepository.ReadOnlyRespository.GetAsync(
                    filter: p => productIds.Contains(p.ID)
                );
                var productLookup = products.ToDictionary(p => p.ID, p => p);

                // Get parent products for child products that have ParentID
                var childProducts = products.Where(p => p.ParentID.HasValue).ToList();
                var parentProductIds = childProducts.Select(p => p.ParentID!.Value).Distinct().ToList();

                var parentProducts = new List<Product>();
                if (parentProductIds.Any())
                {
                    parentProducts = (await _productRepository.ReadOnlyRespository.GetAsync(
                        filter: p => parentProductIds.Contains(p.ID)
                    )).ToList();

                    // Add parent products to lookup if not already present
                    foreach (var parentProduct in parentProducts)
                    {
                        if (!productLookup.ContainsKey(parentProduct.ID))
                        {
                            productLookup[parentProduct.ID] = parentProduct;
                        }
                    }
                }

                // Transform child products to parent products and calculate quantities with loss rate
                var transformedOrderItems = allOrderItems.Select(item =>
                {
                    var product = productLookup.TryGetValue(item.Product_ID, out var prod) ? prod : null;

                    // If product has a parent, use parent product instead
                    var targetProductId = product?.ParentID ?? item.Product_ID;

                    // Calculate quantity with loss rate for all products that have LossRate
                    var adjustedQuantity = item.Quantity;
                    if (product?.LossRate.HasValue == true)
                    {
                        // Apply loss rate: (100 + LossRate) * Quantity / 100
                        adjustedQuantity = (100 + product.LossRate.Value) * item.Quantity / 100;
                    }

                    return new
                    {
                        OriginalProductId = item.Product_ID,
                        TargetProductId = targetProductId,
                        Quantity = adjustedQuantity,
                        Price = item.Price,
                        Unit_ID = item.Unit_ID,
                        POItemId = item.ID
                    };
                }).ToList();

                // Get all target product IDs for vendor relationships
                var allTargetProductIds = transformedOrderItems.Select(i => i.TargetProductId).Distinct().ToList();

                // Get product-vendor relationships for all target products with highest priority vendors
                var productVendors = await _productVendorRepository.GetHighestPriorityVendorsByProductIdsAsync(allTargetProductIds);

                // Create lookup dictionary for better performance - each product maps to its highest priority vendor
                var productVendorLookup = productVendors.ToDictionary(pv => pv.Product_ID, pv => pv.Vendor_ID);

                // Create product-vendor price lookup for getting vendor prices
                var productVendorPriceLookup = productVendors
                    .ToDictionary(pv => $"{pv.Product_ID}_{pv.Vendor_ID}", pv => pv.UnitPrice ?? 0);

                // Group transformed items by vendor using lookup
                var vendorGroups = transformedOrderItems
                    .Where(item => productVendorLookup.ContainsKey(item.TargetProductId))
                    .GroupBy(item => productVendorLookup[item.TargetProductId])
                    .ToDictionary(g => g.Key, g => g.ToList());

                if (!vendorGroups.Any())
                {
                    ack.AddMessage("Không tìm thấy thông tin nhà cung cấp cho các sản phẩm trong đơn hàng.");
                    return ack;
                }

                // Get all vendor information in one query
                var vendorIds = vendorGroups.Keys.ToList();
                var vendors = await _vendorRepository.ReadOnlyRespository.GetAsync(
                    filter: v => vendorIds.Contains(v.ID)
                );
                var vendorLookup = vendors.ToDictionary(v => v.ID, v => v);

                var createdChildPAs = new List<PurchaseAgreementViewModel>();

                // Generate unique GroupCode using dedicated generator
                var groupCode = await Generator.GenerateEntityCodeAsync(EntityPrefix.PAGroupCode, DbContext);

                // Prepare all Purchase Agreements and Items for batch insert
                var purchaseAgreements = new List<Purchase_Agreement>();
                var allAgreementItems = new List<Purchase_Agreement_Item>();
                var currentDateTime = DateTime.Now;

                // Create purchase agreement for each vendor (Child PAs)
                foreach (var vendorGroup in vendorGroups)
                {
                    var vendorId = vendorGroup.Key;
                    var items = vendorGroup.Value;

                    // Group items by target product and sum quantities
                    var productGroups = items.GroupBy(i => i.TargetProductId)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            TotalQuantity = g.Sum(i => i.Quantity),
                            Price = g.First().Price,
                            UnitId = g.First().Unit_ID,
                            POItemIds = string.Join(",", g.Select(i => i.POItemId))
                        }).ToList();

                    // Create purchase agreement (Child PA)
                    var purchaseAgreement = new Purchase_Agreement
                    {
                        Vendor_ID = vendorId,
                        Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.PurchaseAgreement, DbContext),
                        GroupCode = groupCode,
                        TotalPrice = productGroups.Sum(pg =>
                        {
                            var vendorPrice = productVendorPriceLookup.TryGetValue($"{pg.ProductId}_{vendorId}", out var price) ? price : (pg.Price ?? 0);
                            return vendorPrice * pg.TotalQuantity;
                        }),
                        Status = EPAStatus.New.ToString(),
                        CreatedDate = currentDateTime,
                        CreatedBy = CurrentUserId,
                        UpdatedDate = currentDateTime,
                        UpdatedBy = CurrentUserId,
                        IsActive = true
                    };

                    purchaseAgreements.Add(purchaseAgreement);

                    // Prepare agreement items with vendor prices
                    var agreementItems = productGroups.Select(productGroup =>
                    {
                        // Use vendor price if available, fallback to PO item price
                        var vendorPrice = productVendorPriceLookup.TryGetValue($"{productGroup.ProductId}_{vendorId}", out var price) ? price : (productGroup.Price ?? 0);

                        return new Purchase_Agreement_Item
                        {
                            // PA_ID will be set after saving the agreement
                            Product_ID = productGroup.ProductId,
                            Quantity = productGroup.TotalQuantity,
                            Unit_ID = productGroup.UnitId,
                            Price = vendorPrice, // Use vendor price
                            PO_Item_ID_List = productGroup.POItemIds
                        };
                    }).ToList();

                    allAgreementItems.AddRange(agreementItems);
                }

                // Bulk insert all Purchase Agreements using repository without auto-save
                await _purchaseAgreementRepository.Repository.AddRangeWithoutSaveAsync(purchaseAgreements);
                await _purchaseAgreementRepository.Repository.SaveChangesAsync();

                // Set PA_ID for agreement items using a more efficient approach
                var itemIndex = 0;
                foreach (var agreement in purchaseAgreements)
                {
                    var vendorItems = vendorGroups[agreement.Vendor_ID];
                    var productGroupCount = vendorItems.GroupBy(i => i.TargetProductId).Count();

                    for (int i = 0; i < productGroupCount; i++)
                    {
                        allAgreementItems[itemIndex + i].PA_ID = agreement.ID;
                    }
                    itemIndex += productGroupCount;
                }

                // Bulk insert all agreement items using repository without auto-save
                if (allAgreementItems.Count > 0)
                {
                    await _purchaseAgreementItemRepository.Repository.AddRangeWithoutSaveAsync(allAgreementItems);
                    await _purchaseAgreementItemRepository.Repository.SaveChangesAsync();
                }

                // Bulk update purchase orders status to Executed using repository without auto-save
                foreach (var order in confirmedOrders)
                {
                    order.Status = EPOStatus.Executed.ToString();
                    order.UpdatedDate = currentDateTime;
                    order.UpdatedBy = CurrentUserId;
                }

                _purchaseOrderRepository.Repository.UpdateRangeWithoutSave(confirmedOrders);
                await _purchaseOrderRepository.Repository.SaveChangesAsync();

                // Create view models using pre-loaded vendor data
                foreach (var agreement in purchaseAgreements)
                {
                    var agreementViewModel = _mapper.Map<PurchaseAgreementViewModel>(agreement);

                    // Use pre-loaded vendor data
                    if (vendorLookup.TryGetValue(agreement.Vendor_ID, out var vendor))
                    {
                        agreementViewModel.VendorName = vendor.Name;
                    }

                    createdChildPAs.Add(agreementViewModel);
                }

                // Create Parent PA ViewModel
                var paViewModel = new PAGroupViewModel
                {
                    GroupCode = groupCode,
                    TotalPrice = createdChildPAs.Sum(pa => pa.TotalPrice),
                    Status = EPAStatus.New,
                    CreatedDate = DateTime.Now,
                    CreatedBy = CurrentUserId,
                    UpdatedDate = DateTime.Now,
                    UpdatedBy = CurrentUserId,
                    ChildPAs = createdChildPAs
                };

                ack.Data = paViewModel;
                ack.IsSuccess = true;
                ack.AddMessage($"Đã tạo thành công PA tổng hợp với {createdChildPAs.Count} PA con từ {confirmedOrders.Count} đơn hàng.");

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();

                ack.AddMessage($"Lỗi khi tạo PA tổng hợp từ đơn hàng: {ex.Message}");
                _logger.LogError(ex, "Error creating PA from confirmed orders - Transaction rolled back");
            }
            return ack;
        }

        /// <summary>
        /// Alternative implementation using Unit of Work pattern
        /// </summary>
        public async Task<Acknowledgement<PAGroupViewModel>> CreatePAFromConfirmedOrdersWithUoW()
        {
            var ack = new Acknowledgement<PAGroupViewModel>();

            // Create Unit of Work instance
            using var unitOfWork = new UnitOfWork(DbContext, (SampleReadOnlyDBContext)DbContext);

            try
            {
                var result = await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Get all confirmed purchase orders with items in one query
                    var confirmedOrders = await unitOfWork.PurchaseOrders.ReadOnlyRespository.GetAsync(
                        filter: po => po.Status == EPOStatus.Confirmed.ToString() && po.IsActive,
                        includeProperties: "PurchaseOrderItems"
                    );

                    if (!confirmedOrders.Any())
                    {
                        throw new InvalidOperationException("Không có đơn hàng nào ở trạng thái Đã xác nhận để tạo PA tổng hợp.");
                    }

                    // Use included items instead of separate query
                    var allOrderItems = confirmedOrders.SelectMany(po => po.PurchaseOrderItems).ToList();

                    if (!allOrderItems.Any())
                    {
                        throw new InvalidOperationException("Không có sản phẩm nào trong các đơn hàng đã xác nhận.");
                    }

                    // Get product-vendor relationships for all products with highest priority vendors
                    var productIds = allOrderItems.Select(poi => poi.Product_ID).Distinct().ToList();
                    var productVendors = await unitOfWork.ProductVendors.GetHighestPriorityVendorsByProductIdsAsync(productIds);

                    // Create lookup dictionary for better performance - each product maps to its highest priority vendor
                    var productVendorLookup = productVendors.ToDictionary(pv => pv.Product_ID, pv => pv.Vendor_ID);

                    // Group items by vendor using lookup
                    var vendorGroups = allOrderItems
                        .Where(item => productVendorLookup.ContainsKey(item.Product_ID))
                        .GroupBy(item => productVendorLookup[item.Product_ID])
                        .ToDictionary(g => g.Key, g => g.ToList());

                    if (!vendorGroups.Any())
                    {
                        throw new InvalidOperationException("Không tìm thấy thông tin nhà cung cấp cho các sản phẩm trong đơn hàng.");
                    }

                    // Get all vendor information in one query
                    var vendorIds = vendorGroups.Keys.ToList();
                    var vendors = await unitOfWork.Vendors.ReadOnlyRespository.GetAsync(
                        filter: v => vendorIds.Contains(v.ID)
                    );
                    var vendorLookup = vendors.ToDictionary(v => v.ID, v => v);

                    var createdChildPAs = new List<PurchaseAgreementViewModel>();

                    // Generate unique GroupCode using dedicated generator
                    var groupCode = await Generator.GenerateEntityCodeAsync(EntityPrefix.PAGroupCode, DbContext);

                    // Prepare all Purchase Agreements and Items for batch insert
                    var purchaseAgreements = new List<Purchase_Agreement>();
                    var allAgreementItems = new List<Purchase_Agreement_Item>();
                    var currentDateTime = DateTime.Now;

                    // Create purchase agreement for each vendor (Child PAs)
                    foreach (var vendorGroup in vendorGroups)
                    {
                        var vendorId = vendorGroup.Key;
                        var items = vendorGroup.Value;

                        // Group items by product and sum quantities
                        var productGroups = items.GroupBy(i => i.Product_ID)
                            .Select(g => new
                            {
                                ProductId = g.Key,
                                TotalQuantity = g.Sum(i => i.Quantity),
                                Price = g.First().Price,
                                UnitId = g.First().Unit_ID,
                                POItemIds = string.Join(",", g.Select(i => i.ID))
                            }).ToList();

                        // Create purchase agreement (Child PA)
                        var purchaseAgreement = new Purchase_Agreement
                        {
                            Vendor_ID = vendorId,
                            Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.PurchaseAgreement, DbContext),
                            GroupCode = groupCode,
                            TotalPrice = productGroups.Sum(pg => (pg.Price ?? 0) * pg.TotalQuantity),
                            Status = EPAStatus.New.ToString(),
                            CreatedDate = currentDateTime,
                            CreatedBy = CurrentUserId,
                            UpdatedDate = currentDateTime,
                            UpdatedBy = CurrentUserId,
                            IsActive = true
                        };

                        purchaseAgreements.Add(purchaseAgreement);

                        // Prepare agreement items (will set PA_ID after saving agreements)
                        var agreementItems = productGroups.Select(productGroup => new Purchase_Agreement_Item
                        {
                            // PA_ID will be set after saving the agreement
                            Product_ID = productGroup.ProductId,
                            Quantity = productGroup.TotalQuantity,
                            Unit_ID = productGroup.UnitId,
                            Price = productGroup.Price,
                            PO_Item_ID_List = productGroup.POItemIds
                        }).ToList();

                        allAgreementItems.AddRange(agreementItems);
                    }

                    // Bulk insert all Purchase Agreements using repository without auto-save
                    await unitOfWork.PurchaseAgreements.Repository.AddRangeWithoutSaveAsync(purchaseAgreements);
                    await unitOfWork.SaveChangesAsync();

                    // Set PA_ID for agreement items
                    var itemIndex = 0;
                    foreach (var agreement in purchaseAgreements)
                    {
                        var vendorItems = vendorGroups[agreement.Vendor_ID];
                        var productGroupCount = vendorItems.GroupBy(i => i.Product_ID).Count();

                        for (int i = 0; i < productGroupCount; i++)
                        {
                            allAgreementItems[itemIndex + i].PA_ID = agreement.ID;
                        }
                        itemIndex += productGroupCount;
                    }

                    // Bulk insert all agreement items
                    if (allAgreementItems.Count > 0)
                    {
                        await unitOfWork.PurchaseAgreementItems.Repository.AddRangeWithoutSaveAsync(allAgreementItems);
                        await unitOfWork.SaveChangesAsync();
                    }

                    // Bulk update purchase orders status to Executed
                    foreach (var order in confirmedOrders)
                    {
                        order.Status = EPOStatus.Executed.ToString();
                        order.UpdatedDate = currentDateTime;
                        order.UpdatedBy = CurrentUserId;
                    }

                    unitOfWork.PurchaseOrders.Repository.UpdateRangeWithoutSave(confirmedOrders);
                    await unitOfWork.SaveChangesAsync();

                    // Create view models using pre-loaded vendor data
                    foreach (var agreement in purchaseAgreements)
                    {
                        var agreementViewModel = _mapper.Map<PurchaseAgreementViewModel>(agreement);

                        // Use pre-loaded vendor data
                        if (vendorLookup.TryGetValue(agreement.Vendor_ID, out var vendor))
                        {
                            agreementViewModel.VendorName = vendor.Name;
                        }

                        createdChildPAs.Add(agreementViewModel);
                    }

                    // Create Parent PA ViewModel
                    var paViewModel = new PAGroupViewModel
                    {
                        GroupCode = groupCode,
                        TotalPrice = createdChildPAs.Sum(pa => pa.TotalPrice),
                        Status = EPAStatus.New,
                        CreatedDate = DateTime.Now,
                        CreatedBy = CurrentUserId,
                        UpdatedDate = DateTime.Now,
                        UpdatedBy = CurrentUserId,
                        ChildPAs = createdChildPAs
                    };

                    return paViewModel;
                });

                ack.Data = result;
                ack.IsSuccess = true;
                ack.AddMessage($"Đã tạo thành công PA tổng hợp với {result.ChildPAs?.Count ?? 0} PA con từ đơn hàng.");
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi tạo PA tổng hợp từ đơn hàng: {ex.Message}");
                _logger.LogError(ex, "Error creating PA from confirmed orders with UoW - Transaction rolled back");
            }

            return ack;
        }

        public async Task<Acknowledgement<EditablePAGroupPreviewViewModel>> GetEditablePAGroupPreview()
        {
            var ack = new Acknowledgement<EditablePAGroupPreviewViewModel>();
            try
            {
                // Get all confirmed purchase orders with items in one query
                var confirmedOrders = await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                    filter: po => po.Status == EPOStatus.Confirmed.ToString() && po.IsActive,
                    includeProperties: "PurchaseOrderItems"
                );

                if (!confirmedOrders.Any())
                {
                    ack.AddMessage("Không có đơn hàng nào ở trạng thái Đã xác nhận để tạo PA tổng hợp.");
                    return ack;
                }

                // Use included items instead of separate query
                var allOrderItems = confirmedOrders.SelectMany(po => po.PurchaseOrderItems).ToList();

                if (!allOrderItems.Any())
                {
                    ack.AddMessage("Không có sản phẩm nào trong các đơn hàng đã xác nhận.");
                    return ack;
                }

                // Get product information for all products and their parents in one query
                var productIds = allOrderItems.Select(poi => poi.Product_ID).Distinct().ToList();
                var products = await _productRepository.ReadOnlyRespository.GetAsync(
                    filter: p => productIds.Contains(p.ID)
                );

                // Get parent product IDs and fetch all products (original + parents) in one query
                var parentProductIds = products.Where(p => p.ParentID.HasValue)
                                              .Select(p => p.ParentID!.Value)
                                              .Distinct()
                                              .Except(productIds) // Only get parents not already loaded
                                              .ToList();

                if (parentProductIds.Any())
                {
                    var parentProducts = await _productRepository.ReadOnlyRespository.GetAsync(
                        filter: p => parentProductIds.Contains(p.ID)
                    );
                    products = products.Concat(parentProducts).ToList();
                }

                var productLookup = products.ToDictionary(p => p.ID, p => p);

                // Get unit information for all units
                var unitIds = allOrderItems.Where(poi => poi.Unit_ID.HasValue).Select(poi => poi.Unit_ID.Value).Distinct().ToList();
                var units = await _unitRepository.ReadOnlyRespository.GetAsync(
                    filter: u => unitIds.Contains(u.ID)
                );
                var unitLookup = units.ToDictionary(u => u.ID, u => u);

                // Get all available vendors using common service and pre-parse them
                var allVendorsResponse = await _commonService.GetDataOptionsDropdown("", ECategoryType.Vendor);
                var allVendors = allVendorsResponse.IsSuccess ? allVendorsResponse.Data : new List<KendoDropdownListModel<string>>();

                // Pre-parse all vendors to avoid repeated parsing in the loop
                var parsedVendors = allVendors?
                    .Where(vendor => vendor != null && !string.IsNullOrEmpty(vendor.Value) && int.TryParse(vendor.Value, out _))
                    .Select(vendor => new VendorOptionViewModel
                    {
                        ID = int.Parse(vendor.Value),
                        Name = vendor.Text?.Split('(')[0].Trim() ?? "",
                        Code = vendor.Text?.Contains('(') == true ? vendor.Text.Split('(')[1].Replace(")", "").Trim() : ""
                    })
                    .ToList() ?? new List<VendorOptionViewModel>();

                // Transform child products to parent products and calculate quantities with loss rate
                var transformedOrderItems = allOrderItems.Select(item =>
                {
                    var product = productLookup.TryGetValue(item.Product_ID, out var prod) ? prod : null;

                    // If product has a parent, use parent product instead
                    var targetProductId = product?.ParentID ?? item.Product_ID;
                    var targetProduct = productLookup.TryGetValue(targetProductId, out var targetProd) ? targetProd : product;

                    // Calculate quantity with loss rate for all products that have LossRate
                    var adjustedQuantity = item.Quantity;
                    if (product?.LossRate.HasValue == true)
                    {
                        // Apply loss rate: (100 + LossRate) * Quantity / 100
                        adjustedQuantity = (100 + product.LossRate.Value) * item.Quantity / 100;
                    }

                    return new
                    {
                        OriginalProductId = item.Product_ID,
                        TargetProductId = targetProductId,
                        TargetProduct = targetProduct,
                        Quantity = adjustedQuantity,
                        Price = item.Price,
                        UnitId = targetProduct?.Unit_ID ?? item.Unit_ID,
                        POItemId = item.ID
                    };
                }).ToList();

                // Get all target product IDs (including parent products)
                var allTargetProductIds = transformedOrderItems.Select(i => i.TargetProductId).Distinct().ToList();

                // Get product-vendor relationships for all target products in one call
                var allProductVendors = await _productVendorRepository.GetByProductIdsAsync(allTargetProductIds);
                var productVendorLookup = allProductVendors.ToLookup(pv => pv.Product_ID, pv => pv.Vendor_ID);

                // Create default vendor lookup from the same data (highest priority = lowest priority number)
                var defaultVendorLookup = allProductVendors
                    .GroupBy(pv => pv.Product_ID)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(pv => pv.Priority ?? int.MaxValue)
                              .ThenBy(pv => pv.UnitPrice ?? decimal.MaxValue)
                              .First().Vendor_ID
                    );

                // Create product-vendor price lookup for getting vendor prices
                var productVendorPriceLookup = allProductVendors
                    .ToDictionary(pv => $"{pv.Product_ID}_{pv.Vendor_ID}", pv => pv.UnitPrice ?? 0);

                // Group items by target product (parent product) and create mappings
                var productGroups = transformedOrderItems.GroupBy(i => i.TargetProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQuantity = g.Sum(i => i.Quantity),
                        Price = g.First().Price,
                        UnitId = g.First().UnitId,
                        POItemIds = string.Join(",", g.Select(i => i.POItemId))
                    }).ToList();

                var productVendorMappings = new List<ProductVendorMappingViewModel>();

                foreach (var productGroup in productGroups)
                {
                    var product = productLookup.TryGetValue(productGroup.ProductId, out var prod) ? prod : null;
                    var unit = productGroup.UnitId.HasValue && unitLookup.TryGetValue(productGroup.UnitId.Value, out var u) ? u : null;

                    // Get available vendors for this product from product-vendor relationships
                    var availableVendorIds = productVendorLookup.Contains(productGroup.ProductId)
                        ? productVendorLookup[productGroup.ProductId].ToList()
                        : new List<int>();

                    // Filter pre-parsed vendors to only those available for this product and add prices
                    var availableVendors = parsedVendors
                        .Where(vendor => availableVendorIds.Contains(vendor.ID))
                        .Select(vendor =>
                        {
                            // Get price for this vendor-product combination
                            var vendorPrice = productVendorPriceLookup.TryGetValue($"{productGroup.ProductId}_{vendor.ID}", out var price) ? price : 0;

                            return new VendorOptionViewModel
                            {
                                ID = vendor.ID,
                                Name = vendor.Name,
                                Code = vendor.Code,
                                Price = vendorPrice // Add price to vendor option
                            };
                        })
                        .ToList();

                    // Use highest priority vendor as default, fallback to first available vendor
                    var defaultVendorId = defaultVendorLookup.TryGetValue(productGroup.ProductId, out var highestPriorityVendorId)
                        ? highestPriorityVendorId
                        : availableVendorIds.FirstOrDefault();
                    var defaultVendor = availableVendors.FirstOrDefault(v => v.ID == defaultVendorId);

                    // Get vendor price from product-vendor relationship
                    var vendorPrice = defaultVendor != null
                        ? productVendorPriceLookup.TryGetValue($"{productGroup.ProductId}_{defaultVendor.ID}", out var price) ? price : 0
                        : 0;

                    var mapping = new ProductVendorMappingViewModel
                    {
                        Product_ID = productGroup.ProductId,
                        ProductName = product?.Name,
                        ProductCode = product?.Code,
                        TotalQuantity = productGroup.TotalQuantity,
                        Unit_ID = productGroup.UnitId,
                        UnitName = unit?.Name,
                        Price = vendorPrice, // Use vendor price instead of PO item price
                        Vendor_ID = defaultVendor?.ID ?? 0,
                        VendorName = defaultVendor?.Name,
                        AvailableVendors = availableVendors,
                        PO_Item_ID_List = productGroup.POItemIds
                    };

                    productVendorMappings.Add(mapping);
                }

                // Create editable preview
                var editablePreview = new EditablePAGroupPreviewViewModel
                {
                    GroupCode = "[Preview] Sẽ được tạo tự động",
                    TotalPrice = productVendorMappings.Sum(m => m.TotalAmount),
                    Status = EPAStatus.New,
                    ProductVendorMappings = productVendorMappings
                };

                ack.Data = editablePreview;
                ack.IsSuccess = true;
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi lấy preview PA tổng hợp có thể chỉnh sửa: {ex.Message}");
                _logger.LogError(ex, "Error getting editable PA group preview");
            }
            return ack;
        }

        public async Task<Acknowledgement<PAGroupViewModel>> CreatePAGroupWithCustomMapping(CreatePAGroupWithMappingRequest request)
        {
            var ack = new Acknowledgement<PAGroupViewModel>();

            if (request?.ProductVendorMappings == null || !request.ProductVendorMappings.Any())
            {
                ack.AddMessage("Dữ liệu mapping sản phẩm-nhà cung cấp không hợp lệ.");
                return ack;
            }

            // Use database transaction to ensure data consistency
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                // Validate that all products have valid vendor assignments
                var invalidMappings = request.ProductVendorMappings.Where(m => m.Vendor_ID <= 0).ToList();
                if (invalidMappings.Any())
                {
                    ack.AddMessage($"Có {invalidMappings.Count} sản phẩm chưa được gán nhà cung cấp hợp lệ.");
                    return ack;
                }

                // Get all PO Item IDs from mappings
                var allPOItemIds = new List<int>();
                foreach (var mapping in request.ProductVendorMappings)
                {
                    if (!string.IsNullOrEmpty(mapping.PO_Item_ID_List))
                    {
                        var poItemIds = mapping.PO_Item_ID_List.Split(',')
                            .Where(id => int.TryParse(id.Trim(), out _))
                            .Select(id => int.Parse(id.Trim()))
                            .ToList();
                        allPOItemIds.AddRange(poItemIds);
                    }
                }

                // Get related Purchase Orders and update their status to Executed
                var relatedPOIds = new List<int>();
                if (allPOItemIds.Any())
                {
                    var poItems = await _purchaseOrderItemRepository.ReadOnlyRespository.GetAsync(
                        filter: poi => allPOItemIds.Contains(poi.ID)
                    );
                    relatedPOIds = poItems.Select(poi => poi.PO_ID).Distinct().ToList();
                }

                var relatedPOs = new List<Purchase_Order>();
                if (relatedPOIds.Any())
                {
                    relatedPOs = (await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                        filter: po => relatedPOIds.Contains(po.ID) && po.Status == EPOStatus.Confirmed.ToString() && po.IsActive
                    )).ToList();

                    // Update PO status to Executed
                    foreach (var po in relatedPOs)
                    {
                        po.Status = EPOStatus.Executed.ToString();
                        po.UpdatedDate = DateTime.Now;
                        po.UpdatedBy = CurrentUserId;
                    }

                    await _purchaseOrderRepository.Repository.UpdateRangeAsync(relatedPOs);
                    await _purchaseOrderRepository.Repository.SaveChangesAsync();
                }

                // Generate unique GroupCode
                var groupCode = await Generator.GenerateEntityCodeAsync(EntityPrefix.PAGroupCode, DbContext);

                // Group mappings by vendor
                var vendorGroups = request.ProductVendorMappings.GroupBy(m => m.Vendor_ID).ToList();

                // Get vendor information
                var vendorIds = vendorGroups.Select(g => g.Key).ToList();
                var vendors = await _vendorRepository.ReadOnlyRespository.GetAsync(
                    filter: v => vendorIds.Contains(v.ID)
                );
                var vendorLookup = vendors.ToDictionary(v => v.ID, v => v);

                // Get product-vendor relationships for price lookup
                var allProductIds = request.ProductVendorMappings.Select(m => m.Product_ID).Distinct().ToList();
                var allProductVendors = await _productVendorRepository.GetByProductIdsAsync(allProductIds);
                var productVendorPriceLookup = allProductVendors
                    .ToDictionary(pv => $"{pv.Product_ID}_{pv.Vendor_ID}", pv => pv.UnitPrice ?? 0);

                var createdChildPAs = new List<PurchaseAgreementViewModel>();
                var purchaseAgreements = new List<Purchase_Agreement>();
                var allAgreementItems = new List<Purchase_Agreement_Item>();
                var currentDateTime = DateTime.Now;

                // Create purchase agreement for each vendor
                foreach (var vendorGroup in vendorGroups)
                {
                    var vendorId = vendorGroup.Key;
                    var mappings = vendorGroup.ToList();

                    // Calculate total price using vendor prices
                    var totalPrice = mappings.Sum(m =>
                    {
                        var vendorPrice = productVendorPriceLookup.TryGetValue($"{m.Product_ID}_{m.Vendor_ID}", out var price) ? price : (m.Price ?? 0);
                        return (decimal)(m.TotalQuantity * vendorPrice);
                    });

                    // Create purchase agreement
                    var purchaseAgreement = new Purchase_Agreement
                    {
                        Vendor_ID = vendorId,
                        Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.PurchaseAgreement, DbContext),
                        GroupCode = groupCode,
                        TotalPrice = totalPrice, // Use calculated total price with vendor prices
                        Status = EPAStatus.New.ToString(),
                        CreatedDate = currentDateTime,
                        CreatedBy = CurrentUserId,
                        UpdatedDate = currentDateTime,
                        UpdatedBy = CurrentUserId,
                        IsActive = true
                    };

                    purchaseAgreements.Add(purchaseAgreement);

                    // Prepare agreement items with vendor prices
                    var agreementItems = mappings.Select(mapping =>
                    {
                        // Use vendor price if available, fallback to mapping price
                        var vendorPrice = productVendorPriceLookup.TryGetValue($"{mapping.Product_ID}_{mapping.Vendor_ID}", out var price) ? price : mapping.Price;

                        return new Purchase_Agreement_Item
                        {
                            Product_ID = mapping.Product_ID,
                            Quantity = mapping.TotalQuantity,
                            Unit_ID = mapping.Unit_ID,
                            Price = vendorPrice, // Use vendor price
                            PO_Item_ID_List = mapping.PO_Item_ID_List
                        };
                    }).ToList();

                    allAgreementItems.AddRange(agreementItems);
                }

                // Save purchase agreements
                await _purchaseAgreementRepository.Repository.AddRangeAsync(purchaseAgreements);
                await _purchaseAgreementRepository.Repository.SaveChangesAsync();

                // Set PA_ID for agreement items and save them
                var itemIndex = 0;
                foreach (var pa in purchaseAgreements)
                {
                    var vendorMappings = request.ProductVendorMappings.Where(m => m.Vendor_ID == pa.Vendor_ID).ToList();
                    for (int i = 0; i < vendorMappings.Count; i++)
                    {
                        allAgreementItems[itemIndex].PA_ID = pa.ID;
                        itemIndex++;
                    }
                }

                await _purchaseAgreementItemRepository.Repository.AddRangeAsync(allAgreementItems);
                await _purchaseAgreementItemRepository.Repository.SaveChangesAsync();

                // Create response ViewModels
                foreach (var pa in purchaseAgreements)
                {
                    var vendor = vendorLookup.TryGetValue(pa.Vendor_ID, out var v) ? v : null;
                    var vendorMappings = request.ProductVendorMappings.Where(m => m.Vendor_ID == pa.Vendor_ID).ToList();

                    var paItems = vendorMappings.Select(mapping =>
                    {
                        // Use vendor price if available, fallback to mapping price
                        var vendorPrice = productVendorPriceLookup.TryGetValue($"{mapping.Product_ID}_{mapping.Vendor_ID}", out var price) ? price : (mapping.Price ?? 0);

                        return new PurchaseAgreementItemViewModel
                        {
                            Product_ID = mapping.Product_ID,
                            ProductName = mapping.ProductName,
                            Quantity = mapping.TotalQuantity,
                            Unit_ID = mapping.Unit_ID,
                            UnitName = mapping.UnitName,
                            Price = vendorPrice // Use vendor price
                        };
                    }).ToList();

                    var paViewModel = new PurchaseAgreementViewModel
                    {
                        Id = pa.ID,
                        Code = pa.Code,
                        Vendor_ID = pa.Vendor_ID,
                        VendorName = vendor?.Name,
                        GroupCode = pa.GroupCode,
                        TotalPrice = pa.TotalPrice,
                        Status = EPAStatus.New,
                        PurchaseAgreementItems = paItems
                    };

                    createdChildPAs.Add(paViewModel);
                }

                // Create Parent PA ViewModel
                var paGroupViewModel = new PAGroupViewModel
                {
                    GroupCode = groupCode,
                    TotalPrice = createdChildPAs.Sum(pa => pa.TotalPrice),
                    Status = EPAStatus.New,
                    CreatedDate = DateTime.Now,
                    CreatedBy = CurrentUserId,
                    UpdatedDate = DateTime.Now,
                    UpdatedBy = CurrentUserId,
                    ChildPAs = createdChildPAs
                };

                ack.Data = paGroupViewModel;
                ack.IsSuccess = true;
                ack.AddMessage($"Đã tạo thành công PA tổng hợp với {createdChildPAs.Count} PA con từ {relatedPOs.Count} đơn hàng.");

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();

                ack.AddMessage($"Lỗi khi tạo PA tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error creating PA group with custom mapping - Transaction rolled back");
            }
            return ack;
        }

        public async Task<Acknowledgement> SendToVendor(string groupCode)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(groupCode))
                {
                    ack.AddMessage("Mã nhóm không được để trống.");
                    return ack;
                }

                // Get all purchase agreements in this group
                var purchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(
                    filter: pa => pa.GroupCode == groupCode && pa.IsActive
                );

                if (!purchaseAgreements.Any())
                {
                    ack.AddMessage("Không tìm thấy PA tổng hợp với mã nhóm này.");
                    return ack;
                }

                // Check if user has permission to update these PAs using author predicate
                var predicate = PredicateBuilder.New<Purchase_Agreement>(true);
                predicate = predicate.And(p => p.GroupCode == groupCode && p.IsActive);
                predicate = PurchaseAgreementAuthorPredicate.GetPurchaseAgreementAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var authorizedPurchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(filter: predicate);
                if (!authorizedPurchaseAgreements.Any())
                {
                    ack.AddMessage("Bạn không có quyền cập nhật PA tổng hợp này.");
                    return ack;
                }

                // Check if all PAs are in New status (can only send from New status)
                var invalidStatusPAs = purchaseAgreements.Where(pa => pa.Status != EPAStatus.New.ToString()).ToList();
                if (invalidStatusPAs.Any())
                {
                    ack.AddMessage($"Chỉ có thể gửi PA tổng hợp ở trạng thái 'Mới'. Có {invalidStatusPAs.Count} PA không ở trạng thái phù hợp.");
                    return ack;
                }

                // Update all PAs in the group to SendVendor status
                foreach (var pa in purchaseAgreements)
                {
                    pa.Status = EPAStatus.SendVendor.ToString();
                    pa.UpdatedDate = DateTime.Now;
                    pa.UpdatedBy = CurrentUserId;
                }

                // Save changes
                await ack.TrySaveChangesAsync(res => res.UpdateRangeAsync(purchaseAgreements), _purchaseAgreementRepository.Repository);

                if (ack.IsSuccess)
                {
                    ack.AddMessage($"Đã gửi PA tổng hợp '{groupCode}' cho nhà cung cấp thành công.");
                }
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi gửi PA tổng hợp cho nhà cung cấp: {ex.Message}");
                _logger.LogError(ex, "Error sending PA group to vendor");
            }
            return ack;
        }

        public async Task<Acknowledgement> CompletePAGroup(string groupCode)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(groupCode))
                {
                    ack.AddMessage("Mã nhóm không được để trống.");
                    return ack;
                }

                // Get all purchase agreements in this group
                var purchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(
                    filter: pa => pa.GroupCode == groupCode && pa.IsActive
                );

                if (!purchaseAgreements.Any())
                {
                    ack.AddMessage("Không tìm thấy PA tổng hợp với mã nhóm này.");
                    return ack;
                }

                // Check if user has permission to update these PAs using author predicate
                var predicate = PredicateBuilder.New<Purchase_Agreement>(true);
                predicate = predicate.And(p => p.GroupCode == groupCode && p.IsActive);
                predicate = PurchaseAgreementAuthorPredicate.GetPurchaseAgreementAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var authorizedPurchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(filter: predicate);
                if (!authorizedPurchaseAgreements.Any())
                {
                    ack.AddMessage("Bạn không có quyền cập nhật PA tổng hợp này.");
                    return ack;
                }

                // Check if all PAs are in SendVendor status (can only complete from SendVendor status)
                var invalidStatusPAs = purchaseAgreements.Where(pa => pa.Status != EPAStatus.SendVendor.ToString()).ToList();
                if (invalidStatusPAs.Any())
                {
                    ack.AddMessage($"Chỉ có thể hoàn thành PA tổng hợp ở trạng thái 'Đã gửi NCC'. Có {invalidStatusPAs.Count} PA không ở trạng thái phù hợp.");
                    return ack;
                }

                // Update all PAs in the group to Completed status
                foreach (var pa in purchaseAgreements)
                {
                    pa.Status = EPAStatus.Completed.ToString();
                    pa.UpdatedDate = DateTime.Now;
                    pa.UpdatedBy = CurrentUserId;
                }

                // Save changes
                await ack.TrySaveChangesAsync(res => res.UpdateRangeAsync(purchaseAgreements), _purchaseAgreementRepository.Repository);

                if (ack.IsSuccess)
                {
                    ack.AddMessage($"Đã hoàn thành PA tổng hợp '{groupCode}' thành công.");
                }
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi hoàn thành PA tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error completing PA group");
            }
            return ack;
        }

        public async Task<Acknowledgement> CancelPAGroup(string groupCode)
        {
            var ack = new Acknowledgement();

            // Use database transaction to ensure data consistency
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(groupCode))
                {
                    ack.AddMessage("Mã nhóm không được để trống.");
                    return ack;
                }

                // Get all PAs in the group with their items
                var purchaseAgreements = await _purchaseAgreementRepository.ReadOnlyRespository.GetAsync(
                    filter: pa => pa.GroupCode == groupCode && pa.IsActive,
                    includeProperties: "PurchaseAgreementItems"
                );

                if (!purchaseAgreements.Any())
                {
                    ack.AddMessage("Không tìm thấy PA tổng hợp với mã nhóm này.");
                    return ack;
                }

                // Check if any PA is already cancelled
                var alreadyCancelledPAs = purchaseAgreements.Where(pa => pa.Status == EPAStatus.Cancel.ToString()).ToList();
                if (alreadyCancelledPAs.Any())
                {
                    ack.AddMessage("PA tổng hợp đã được hủy trước đó.");
                    return ack;
                }

                // Check if any PA is completed (cannot cancel completed PAs)
                var completedPAs = purchaseAgreements.Where(pa => pa.Status == EPAStatus.Completed.ToString()).ToList();
                if (completedPAs.Any())
                {
                    ack.AddMessage("Không thể hủy PA tổng hợp đã hoàn thành.");
                    return ack;
                }

                // Get all PO Item IDs from PA Items to find related POs
                var allPOItemIds = new List<int>();
                foreach (var pa in purchaseAgreements)
                {
                    if (pa.PurchaseAgreementItems != null)
                    {
                        foreach (var paItem in pa.PurchaseAgreementItems)
                        {
                            if (!string.IsNullOrEmpty(paItem.PO_Item_ID_List))
                            {
                                // Parse comma-separated PO Item IDs
                                var poItemIds = paItem.PO_Item_ID_List.Split(',')
                                    .Where(id => int.TryParse(id.Trim(), out _))
                                    .Select(id => int.Parse(id.Trim()))
                                    .ToList();
                                allPOItemIds.AddRange(poItemIds);
                            }
                        }
                    }
                }

                // Get unique PO IDs from PO Items
                var relatedPOIds = new List<int>();
                if (allPOItemIds.Any())
                {
                    var poItems = await _purchaseOrderItemRepository.ReadOnlyRespository.GetAsync(
                        filter: poi => allPOItemIds.Contains(poi.ID)
                    );
                    relatedPOIds = poItems.Select(poi => poi.PO_ID).Distinct().ToList();
                }

                // Get related Purchase Orders that are currently in Executed status
                var relatedPOs = new List<Purchase_Order>();
                if (relatedPOIds.Any())
                {
                    relatedPOs = (await _purchaseOrderRepository.ReadOnlyRespository.GetAsync(
                        filter: po => relatedPOIds.Contains(po.ID) && po.Status == EPOStatus.Executed.ToString() && po.IsActive
                    )).ToList();
                }

                // Update all PAs in the group to Cancel status
                foreach (var pa in purchaseAgreements)
                {
                    pa.Status = EPAStatus.Cancel.ToString();
                    pa.UpdatedDate = DateTime.Now;
                    pa.UpdatedBy = CurrentUserId;
                }

                // Revert related POs back to Confirmed status
                foreach (var po in relatedPOs)
                {
                    po.Status = EPOStatus.Confirmed.ToString();
                    po.UpdatedDate = DateTime.Now;
                    po.UpdatedBy = CurrentUserId;
                }

                // Save PA changes
                await ack.TrySaveChangesAsync(res => res.UpdateRangeAsync(purchaseAgreements), _purchaseAgreementRepository.Repository);

                if (!ack.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return ack;
                }

                // Save PO changes
                if (relatedPOs.Any())
                {
                    await _purchaseOrderRepository.Repository.UpdateRangeAsync(relatedPOs);
                    await _purchaseOrderRepository.Repository.SaveChangesAsync();
                }

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                if (ack.IsSuccess)
                {
                    var poCount = relatedPOs.Count;
                    ack.AddSuccessMessages($"Đã hủy PA tổng hợp '{groupCode}' thành công. {poCount} đơn hàng PO đã được chuyển về trạng thái 'Đã xác nhận'.");
                }
            }
            catch (Exception ex)
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();

                ack.AddMessage($"Lỗi khi hủy PA tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error cancelling PA group - Transaction rolled back");
            }
            return ack;
        }

        /// <summary>
        /// Transform child products to parent products and calculate quantities with loss rate
        /// </summary>
        private async Task<(List<dynamic> transformedItems, Dictionary<int, Product> productLookup)> TransformOrderItemsToParentProducts(
            List<Purchase_Order_Item> orderItems)
        {
            // Get product information for all products
            var productIds = orderItems.Select(poi => poi.Product_ID).Distinct().ToList();
            var products = await _productRepository.ReadOnlyRespository.GetAsync(
                filter: p => productIds.Contains(p.ID)
            );

            // Get parent product IDs and fetch all products (original + parents) in one query
            var parentProductIds = products.Where(p => p.ParentID.HasValue)
                                          .Select(p => p.ParentID!.Value)
                                          .Distinct()
                                          .Except(productIds) // Only get parents not already loaded
                                          .ToList();

            if (parentProductIds.Any())
            {
                var parentProducts = await _productRepository.ReadOnlyRespository.GetAsync(
                    filter: p => parentProductIds.Contains(p.ID)
                );
                products = products.Concat(parentProducts).ToList();
            }

            var productLookup = products.ToDictionary(p => p.ID, p => p);

            // Transform child products to parent products and calculate quantities with loss rate
            var transformedItems = orderItems.Select(item =>
            {
                var product = productLookup.TryGetValue(item.Product_ID, out var prod) ? prod : null;

                // If product has a parent, use parent product instead
                var targetProductId = product?.ParentID ?? item.Product_ID;
                var targetProduct = productLookup.TryGetValue(targetProductId, out var targetProd) ? targetProd : product;

                // Calculate quantity with loss rate for all products that have LossRate
                var adjustedQuantity = item.Quantity;
                if (product?.LossRate.HasValue == true)
                {
                    // Apply loss rate: (100 + LossRate) * Quantity / 100
                    adjustedQuantity = (100 + product.LossRate.Value) * item.Quantity / 100;
                }

                return new
                {
                    OriginalProductId = item.Product_ID,
                    TargetProductId = targetProductId,
                    TargetProduct = targetProduct,
                    Quantity = adjustedQuantity,
                    Price = item.Price,
                    UnitId = targetProduct?.Unit_ID ?? item.Unit_ID,
                    Unit_ID = item.Unit_ID,
                    TaxRate = item.TaxRate,
                    ProcessingFee = item.ProcessingFee,
                    POItemId = item.ID
                };
            }).ToList();

            return (transformedItems.Cast<dynamic>().ToList(), productLookup);
        }



        private async Task SavePurchaseAgreementItems(int purchaseAgreementId, List<PurchaseAgreementItemViewModel>? items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    var agreementItem = _mapper.Map<Purchase_Agreement_Item>(item);
                    agreementItem.PA_ID = purchaseAgreementId;
                    await _purchaseAgreementItemRepository.Repository.AddAsync(agreementItem);
                }
                await DbContext.SaveChangesAsync();
            }
        }
    }
}
