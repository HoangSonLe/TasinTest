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
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi lấy thông tin PA tổng hợp: {ex.Message}");
                _logger.LogError(ex, "Error getting PA by group code");
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
                    filter: po => po.Status == ((int)EPOStatus.Confirmed).ToString() && po.IsActive,
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

                // Get product-vendor relationships for all products
                var productIds = allOrderItems.Select(poi => poi.Product_ID).Distinct().ToList();
                var productVendors = await _productVendorRepository.GetByProductIdsAsync(productIds);

                // Create lookup dictionary for better performance
                var productVendorLookup = productVendors.ToLookup(pv => pv.Product_ID, pv => pv.Vendor_ID);

                // Group items by vendor using lookup
                var vendorGroups = allOrderItems
                    .Where(item => productVendorLookup.Contains(item.Product_ID))
                    .GroupBy(item => productVendorLookup[item.Product_ID].First())
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
                await _purchaseAgreementRepository.Repository.AddRangeWithoutSaveAsync(purchaseAgreements);
                await _purchaseAgreementRepository.Repository.SaveChangesAsync();

                // Set PA_ID for agreement items using a more efficient approach
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

                    // Get product-vendor relationships for all products
                    var productIds = allOrderItems.Select(poi => poi.Product_ID).Distinct().ToList();
                    var productVendors = await unitOfWork.ProductVendors.GetByProductIdsAsync(productIds);

                    // Create lookup dictionary for better performance
                    var productVendorLookup = productVendors.ToLookup(pv => pv.Product_ID, pv => pv.Vendor_ID);

                    // Group items by vendor using lookup
                    var vendorGroups = allOrderItems
                        .Where(item => productVendorLookup.Contains(item.Product_ID))
                        .GroupBy(item => productVendorLookup[item.Product_ID].First())
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
                ack.AddMessage($"Đã tạo thành công PA tổng hợp với {result.ChildPAs.Count} PA con từ đơn hàng.");
            }
            catch (Exception ex)
            {
                ack.AddMessage($"Lỗi khi tạo PA tổng hợp từ đơn hàng: {ex.Message}");
                _logger.LogError(ex, "Error creating PA from confirmed orders with UoW - Transaction rolled back");
            }

            return ack;
        }



        private async Task SavePurchaseAgreementItems(int purchaseAgreementId, List<PurchaseAgreementItemViewModel> items)
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
