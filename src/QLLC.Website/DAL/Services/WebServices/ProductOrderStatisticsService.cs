using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Services;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebServices
{
    public class ProductOrderStatisticsService : BaseService<ProductOrderStatisticsService>, IProductOrderStatisticsService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProductOrderStatisticsService(
            ILogger<ProductOrderStatisticsService> logger,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper,
            IUnitOfWork unitOfWork
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<ProductOrderStatisticsViewModel>>>> GetProductOrderStatistics(ProductOrderStatisticsSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<ProductOrderStatisticsViewModel>>>();
            try
            {
                // Build predicate for Purchase Agreements with Completed status
                var paPredicate = PredicateBuilder.New<Purchase_Agreement>(pa =>
                    pa.IsActive == true &&
                    pa.Status == EPAStatus.Completed.ToString());

                // Apply date filters
                if (searchModel.DateFrom.HasValue)
                {
                    paPredicate = paPredicate.And(pa => pa.CreatedDate >= searchModel.DateFrom.Value);
                }

                if (searchModel.DateTo.HasValue)
                {
                    paPredicate = paPredicate.And(pa => pa.CreatedDate <= searchModel.DateTo.Value);
                }

                // Apply vendor filter
                if (searchModel.Vendor_ID.HasValue)
                {
                    paPredicate = paPredicate.And(pa => pa.Vendor_ID == searchModel.Vendor_ID.Value);
                }

                // Get completed PAs with their items, products, vendors, and units
                var completedPAs = await _unitOfWork.PurchaseAgreements.ReadOnlyRespository.GetAsync(
                    filter: paPredicate,
                    includeProperties: "PurchaseAgreementItems,Vendor"
                );

                if (!completedPAs.Any())
                {
                    response.Data = new JsonResultPaging<List<ProductOrderStatisticsViewModel>>
                    {
                        Data = new List<ProductOrderStatisticsViewModel>(),
                        PageNumber = searchModel.PageNumber,
                        PageSize = searchModel.PageSize,
                        Total = 0
                    };
                    response.IsSuccess = true;
                    return response;
                }

                // Get all PA items for the completed PAs
                var paIds = completedPAs.Select(pa => pa.ID).ToList();
                var paItems = await _unitOfWork.PurchaseAgreementItems.ReadOnlyRespository.GetAsync(
                    filter: item => paIds.Contains(item.PA_ID)
                );

                // Get all products and units referenced in PA items
                var productIds = paItems.Select(item => item.Product_ID).Distinct().ToList();
                var unitIds = paItems.Where(item => item.Unit_ID.HasValue).Select(item => item.Unit_ID!.Value).Distinct().ToList();

                var products = await DbContext.Products.Where(p => productIds.Contains(p.ID)).ToListAsync();
                var units = await DbContext.Units.Where(u => unitIds.Contains(u.ID)).ToListAsync();

                // Apply product name filter
                if (!string.IsNullOrEmpty(searchModel.ProductName))
                {
                    var productNameNonUnicode = Utils.NonUnicode(searchModel.ProductName.Trim().ToLower());
                    var filteredProductIds = products.Where(p =>
                        p.NameNonUnicode.ToLower().Contains(productNameNonUnicode) ||
                        p.Name.ToLower().Contains(searchModel.ProductName.Trim().ToLower()) ||
                        p.Code.ToLower().Contains(searchModel.ProductName.Trim().ToLower())
                    ).Select(p => p.ID).ToList();

                    paItems = paItems.Where(item => filteredProductIds.Contains(item.Product_ID)).ToList();

                    // Update PA list to only include PAs that have the filtered products
                    var filteredPAIds = paItems.Select(item => item.PA_ID).Distinct().ToList();
                    completedPAs = completedPAs.Where(pa => filteredPAIds.Contains(pa.ID)).ToList();
                }

                // Group by vendor
                var vendorGroups = completedPAs.GroupBy(pa => pa.Vendor_ID).ToList();

                var statisticsViewModels = new List<ProductOrderStatisticsViewModel>();

                foreach (var vendorGroup in vendorGroups)
                {
                    var vendor = vendorGroup.First().Vendor;
                    if (vendor == null) continue;

                    var vendorPAs = vendorGroup.ToList();
                    var vendorPAIds = vendorPAs.Select(pa => pa.ID).ToList();
                    var vendorPAItems = paItems.Where(item => vendorPAIds.Contains(item.PA_ID)).ToList();

                    // Group by product within this vendor
                    var productGroups = vendorPAItems.GroupBy(item => item.Product_ID).ToList();

                    var productStatistics = new List<ProductStatisticsViewModel>();

                    foreach (var productGroup in productGroups)
                    {
                        var product = products.FirstOrDefault(p => p.ID == productGroup.Key);
                        if (product == null) continue;

                        var productItems = productGroup.ToList();
                        var totalQuantity = productItems.Sum(item => item.Quantity);
                        var totalValue = productItems.Sum(item => (item.Price ?? 0) * item.Quantity);
                        var totalOrderAmount = totalValue; // Tổng tiền đặt hàng = tổng giá trị

                        // Tính toán các loại giá
                        var pricesWithValues = productItems.Where(item => item.Price.HasValue && item.Price.Value > 0).ToList();
                        var minPrice = pricesWithValues.Any() ? pricesWithValues.Min(item => item.Price!.Value) : (decimal?)null;
                        var maxPrice = pricesWithValues.Any() ? pricesWithValues.Max(item => item.Price!.Value) : (decimal?)null;
                        var averagePrice = pricesWithValues.Any() ? pricesWithValues.Average(item => item.Price!.Value) : (decimal?)null;

                        // Giá hiện tại = giá của PA được tạo gần nhất
                        var currentPrice = (decimal?)null;
                        if (pricesWithValues.Any())
                        {
                            var latestPA = vendorPAs.Where(pa => productItems.Any(item => item.PA_ID == pa.ID && item.Price.HasValue))
                                                   .OrderByDescending(pa => pa.CreatedDate)
                                                   .FirstOrDefault();
                            if (latestPA != null)
                            {
                                var latestItem = productItems.FirstOrDefault(item => item.PA_ID == latestPA.ID && item.Price.HasValue);
                                currentPrice = latestItem?.Price;
                            }
                        }

                        var paDetails = new List<PAProductDetailViewModel>();

                        if (searchModel.IncludeDetails)
                        {
                            foreach (var item in productItems)
                            {
                                var pa = vendorPAs.FirstOrDefault(p => p.ID == item.PA_ID);
                                if (pa != null)
                                {
                                    paDetails.Add(new PAProductDetailViewModel
                                    {
                                        PA_ID = pa.ID,
                                        PACode = pa.Code,
                                        GroupCode = pa.GroupCode,
                                        Quantity = item.Quantity,
                                        Price = item.Price,
                                        Value = (item.Price ?? 0) * item.Quantity,
                                        CreatedDate = pa.CreatedDate
                                    });
                                }
                            }
                        }

                        var unit = productItems.FirstOrDefault(item => item.Unit_ID.HasValue)?.Unit_ID.HasValue == true
                            ? units.FirstOrDefault(u => u.ID == productItems.First(item => item.Unit_ID.HasValue).Unit_ID!.Value)
                            : null;

                        productStatistics.Add(new ProductStatisticsViewModel
                        {
                            ProductID = product.ID,
                            ProductCode = product.Code,
                            ProductName = product.Name,
                            UnitName = unit?.Name,
                            TotalQuantity = totalQuantity,
                            MinPrice = minPrice,
                            MaxPrice = maxPrice,
                            AveragePrice = averagePrice,
                            CurrentPrice = currentPrice,
                            TotalValue = totalValue,
                            TotalOrderAmount = totalOrderAmount,
                            PACount = productItems.Select(item => item.PA_ID).Distinct().Count(),
                            PADetails = paDetails
                        });
                    }

                    statisticsViewModels.Add(new ProductOrderStatisticsViewModel
                    {
                        Vendor = new VendorStatisticsViewModel
                        {
                            ID = vendor.ID,
                            Code = vendor.Code,
                            Name = vendor.Name,
                            Address = vendor.Address
                        },
                        Products = productStatistics.OrderBy(p => p.ProductName).ToList(),
                        TotalValue = productStatistics.Sum(p => p.TotalValue),
                        TotalOrderAmount = productStatistics.Sum(p => p.TotalOrderAmount),
                        TotalQuantity = productStatistics.Sum(p => p.TotalQuantity),
                        CompletedPACount = vendorPAs.Count
                    });
                }

                // Apply pagination
                var totalRecords = statisticsViewModels.Count;
                var pagedData = statisticsViewModels
                    .OrderBy(s => s.Vendor.Name)
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .ToList();

                response.Data = new JsonResultPaging<List<ProductOrderStatisticsViewModel>>
                {
                    Data = pagedData,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = totalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetProductOrderStatistics: {ex.Message}");
                return response;
            }
        }
    }
}
