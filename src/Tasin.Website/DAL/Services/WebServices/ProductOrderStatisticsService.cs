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
                // Use optimized single query with proper JOINs
                var query = from pa in DbContext.PurchaseAgreements
                            join vendor in DbContext.Vendors on pa.Vendor_ID equals vendor.ID
                            join paItem in DbContext.PurchaseAgreementItems on pa.ID equals paItem.PA_ID
                            join product in DbContext.Products on paItem.Product_ID equals product.ID
                            join unit in DbContext.Units on paItem.Unit_ID equals unit.ID into unitGroup
                            from unit in unitGroup.DefaultIfEmpty()
                            where pa.IsActive == true &&
                                  pa.Status == EPAStatus.Completed.ToString()
                            select new
                            {
                                PA = pa,
                                Vendor = vendor,
                                PAItem = paItem,
                                Product = product,
                                Unit = unit
                            };

                // Apply filters
                if (searchModel.DateFrom.HasValue)
                {
                    query = query.Where(x => x.PA.CreatedDate >= searchModel.DateFrom.Value);
                }

                if (searchModel.DateTo.HasValue)
                {
                    query = query.Where(x => x.PA.CreatedDate <= searchModel.DateTo.Value);
                }

                if (searchModel.Vendor_ID.HasValue)
                {
                    query = query.Where(x => x.PA.Vendor_ID == searchModel.Vendor_ID.Value);
                }

                // Apply product name filter if provided
                if (!string.IsNullOrWhiteSpace(searchModel.ProductName))
                {
                    var productNameFilter = searchModel.ProductName.Trim().ToLower();
                    query = query.Where(x => x.Product.Name.ToLower().Contains(productNameFilter) ||
                                           (x.Product.NameNonUnicode != null && x.Product.NameNonUnicode.ToLower().Contains(productNameFilter)));
                }

                // Execute query and group in database
                var rawData = await query.ToListAsync();

                if (!rawData.Any())
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

                // Efficient in-memory grouping using the optimized data
                var vendorGroups = rawData
                    .GroupBy(x => new { x.Vendor.ID, x.Vendor.Code, x.Vendor.Name, x.Vendor.Address })
                    .ToList();

                var statisticsViewModels = new List<ProductOrderStatisticsViewModel>();

                foreach (var vendorGroup in vendorGroups)
                {
                    var vendorKey = vendorGroup.Key;
                    var vendorData = vendorGroup.ToList();

                    // Group by product within this vendor
                    var productGroups = vendorData
                        .GroupBy(x => new { x.Product.ID, x.Product.Code, x.Product.Name, UnitName = x.Unit?.Name })
                        .ToList();

                    var productStatistics = new List<ProductStatisticsViewModel>();

                    foreach (var productGroup in productGroups)
                    {
                        var productKey = productGroup.Key;
                        var productItems = productGroup.ToList();

                        // Efficient calculations using LINQ aggregations
                        var totalQuantity = productItems.Sum(x => x.PAItem.Quantity);
                        var totalValue = productItems.Sum(x => (x.PAItem.Price ?? 0) * x.PAItem.Quantity);
                        var totalOrderAmount = totalValue;

                        // Calculate price statistics efficiently
                        var validPrices = productItems
                            .Where(x => x.PAItem.Price.HasValue && x.PAItem.Price.Value > 0)
                            .Select(x => x.PAItem.Price!.Value)
                            .ToList();

                        var minPrice = validPrices.Any() ? validPrices.Min() : (decimal?)null;
                        var maxPrice = validPrices.Any() ? validPrices.Max() : (decimal?)null;
                        var averagePrice = validPrices.Any() ? validPrices.Average() : (decimal?)null;

                        // Current price = latest PA price
                        var currentPrice = productItems
                            .Where(x => x.PAItem.Price.HasValue && x.PAItem.Price.Value > 0)
                            .OrderByDescending(x => x.PA.CreatedDate)
                            .FirstOrDefault()?.PAItem.Price;

                        // Efficient PA details creation
                        var paDetails = searchModel.IncludeDetails
                            ? productItems.Select(x => new PAProductDetailViewModel
                            {
                                PA_ID = x.PA.ID,
                                PACode = x.PA.Code,
                                GroupCode = x.PA.GroupCode,
                                Quantity = x.PAItem.Quantity,
                                Price = x.PAItem.Price,
                                Value = (x.PAItem.Price ?? 0) * x.PAItem.Quantity,
                                CreatedDate = x.PA.CreatedDate
                            }).ToList()
                            : new List<PAProductDetailViewModel>();

                        productStatistics.Add(new ProductStatisticsViewModel
                        {
                            ProductID = productKey.ID,
                            ProductCode = productKey.Code,
                            ProductName = productKey.Name,
                            UnitName = productKey.UnitName,
                            TotalQuantity = totalQuantity,
                            MinPrice = minPrice,
                            MaxPrice = maxPrice,
                            AveragePrice = averagePrice,
                            CurrentPrice = currentPrice,
                            TotalValue = totalValue,
                            TotalOrderAmount = totalOrderAmount,
                            PACount = productItems.Select(x => x.PA.ID).Distinct().Count(),
                            PADetails = paDetails
                        });
                    }

                    // Efficient vendor statistics calculation
                    var orderedProducts = productStatistics.OrderBy(p => p.ProductName).ToList();
                    var completedPACount = vendorData.Select(x => x.PA.ID).Distinct().Count();

                    statisticsViewModels.Add(new ProductOrderStatisticsViewModel
                    {
                        Vendor = new VendorStatisticsViewModel
                        {
                            ID = vendorKey.ID,
                            Code = vendorKey.Code,
                            Name = vendorKey.Name,
                            Address = vendorKey.Address
                        },
                        Products = orderedProducts,
                        TotalValue = orderedProducts.Sum(p => p.TotalValue),
                        TotalOrderAmount = orderedProducts.Sum(p => p.TotalOrderAmount),
                        TotalQuantity = orderedProducts.Sum(p => p.TotalQuantity),
                        CompletedPACount = completedPACount
                    });
                }

                // Efficient pagination with early ordering
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

        public async Task<Acknowledgement<JsonResultPaging<List<CustomerProductOrderStatisticsViewModel>>>> GetCustomerProductOrderStatistics(CustomerProductOrderStatisticsSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<CustomerProductOrderStatisticsViewModel>>>();
            try
            {
                // Use optimized single query with proper JOINs for Purchase Orders with Executed status (đã tạo đơn tổng hợp)
                var query = from po in DbContext.PurchaseOrders
                            join customer in DbContext.Customers on po.Customer_ID equals customer.ID
                            join poItem in DbContext.PurchaseOrderItems on po.ID equals poItem.PO_ID
                            join product in DbContext.Products on poItem.Product_ID equals product.ID
                            join unit in DbContext.Units on poItem.Unit_ID equals unit.ID into unitGroup
                            from unit in unitGroup.DefaultIfEmpty()
                            where po.IsActive == true &&
                                  po.Status == EPOStatus.Executed.ToString()
                            select new
                            {
                                PO = po,
                                Customer = customer,
                                POItem = poItem,
                                Product = product,
                                Unit = unit
                            };

                // Apply filters
                if (searchModel.DateFrom.HasValue)
                {
                    query = query.Where(x => x.PO.CreatedDate >= searchModel.DateFrom.Value);
                }

                if (searchModel.DateTo.HasValue)
                {
                    query = query.Where(x => x.PO.CreatedDate <= searchModel.DateTo.Value);
                }

                if (searchModel.Customer_ID.HasValue)
                {
                    query = query.Where(x => x.PO.Customer_ID == searchModel.Customer_ID.Value);
                }

                // Apply product name filter if provided
                if (!string.IsNullOrWhiteSpace(searchModel.ProductName))
                {
                    var productNameFilter = searchModel.ProductName.Trim().ToLower();
                    query = query.Where(x => x.Product.Name.ToLower().Contains(productNameFilter) ||
                                           (x.Product.NameNonUnicode != null && x.Product.NameNonUnicode.ToLower().Contains(productNameFilter)));
                }

                // Execute query and group in database
                var rawData = await query.ToListAsync();

                if (!rawData.Any())
                {
                    response.Data = new JsonResultPaging<List<CustomerProductOrderStatisticsViewModel>>
                    {
                        Data = new List<CustomerProductOrderStatisticsViewModel>(),
                        PageNumber = searchModel.PageNumber,
                        PageSize = searchModel.PageSize,
                        Total = 0
                    };
                    response.IsSuccess = true;
                    return response;
                }

                // Efficient in-memory grouping using the optimized data
                var customerGroups = rawData
                    .GroupBy(x => new { x.Customer.ID, x.Customer.Code, x.Customer.Name, x.Customer.Email, x.Customer.PhoneContact, x.Customer.Address, x.Customer.TaxCode })
                    .ToList();

                var statisticsViewModels = new List<CustomerProductOrderStatisticsViewModel>();

                foreach (var customerGroup in customerGroups)
                {
                    var customerKey = customerGroup.Key;
                    var customerData = customerGroup.ToList();

                    // Group by product within this customer
                    var productGroups = customerData
                        .GroupBy(x => new { x.Product.ID, x.Product.Code, x.Product.Name, UnitName = x.Unit?.Name })
                        .ToList();

                    var productStatistics = new List<CustomerProductStatisticsViewModel>();

                    foreach (var productGroup in productGroups)
                    {
                        var productKey = productGroup.Key;
                        var productItems = productGroup.ToList();

                        // Efficient calculations using LINQ aggregations
                        var totalQuantity = productItems.Sum(x => x.POItem.Quantity);
                        var totalValue = productItems.Sum(x => (x.POItem.Price ?? 0) * x.POItem.Quantity);
                        var totalOrderAmount = totalValue;

                        // Calculate price statistics efficiently
                        var validPrices = productItems
                            .Where(x => x.POItem.Price.HasValue && x.POItem.Price.Value > 0)
                            .Select(x => x.POItem.Price!.Value)
                            .ToList();

                        var minPrice = validPrices.Any() ? validPrices.Min() : (decimal?)null;
                        var maxPrice = validPrices.Any() ? validPrices.Max() : (decimal?)null;
                        var averagePrice = validPrices.Any() ? validPrices.Average() : (decimal?)null;

                        // Current price = latest PO price
                        var currentPrice = productItems
                            .Where(x => x.POItem.Price.HasValue && x.POItem.Price.Value > 0)
                            .OrderByDescending(x => x.PO.CreatedDate)
                            .FirstOrDefault()?.POItem.Price;

                        // Efficient PO details creation
                        var poDetails = searchModel.IncludeDetails
                            ? productItems.Select(x => new POProductDetailViewModel
                            {
                                PO_ID = x.PO.ID,
                                POCode = x.PO.Code,
                                Quantity = x.POItem.Quantity,
                                Price = x.POItem.Price,
                                Value = (x.POItem.Price ?? 0) * x.POItem.Quantity,
                                CreatedDate = x.PO.CreatedDate
                            }).ToList()
                            : new List<POProductDetailViewModel>();

                        productStatistics.Add(new CustomerProductStatisticsViewModel
                        {
                            ProductID = productKey.ID,
                            ProductCode = productKey.Code,
                            ProductName = productKey.Name,
                            UnitName = productKey.UnitName,
                            TotalQuantity = totalQuantity,
                            MinPrice = minPrice,
                            MaxPrice = maxPrice,
                            AveragePrice = averagePrice,
                            CurrentPrice = currentPrice,
                            TotalValue = totalValue,
                            TotalOrderAmount = totalOrderAmount,
                            POCount = productItems.Select(x => x.PO.ID).Distinct().Count(),
                            PODetails = poDetails
                        });
                    }

                    // Efficient customer statistics calculation
                    var orderedProducts = productStatistics.OrderBy(p => p.ProductName).ToList();
                    var executedPOCount = customerData.Select(x => x.PO.ID).Distinct().Count();

                    statisticsViewModels.Add(new CustomerProductOrderStatisticsViewModel
                    {
                        Customer = new CustomerStatisticsViewModel
                        {
                            ID = customerKey.ID,
                            Code = customerKey.Code,
                            Name = customerKey.Name,
                            Email = customerKey.Email,
                            PhoneContact = customerKey.PhoneContact,
                            Address = customerKey.Address,
                            TaxCode = customerKey.TaxCode
                        },
                        Products = orderedProducts,
                        TotalValue = orderedProducts.Sum(p => p.TotalValue),
                        TotalOrderAmount = orderedProducts.Sum(p => p.TotalOrderAmount),
                        ConfirmedPOCount = executedPOCount
                    });
                }

                // Efficient pagination with early ordering
                var totalRecords = statisticsViewModels.Count;
                var pagedData = statisticsViewModels
                    .OrderBy(s => s.Customer.Name)
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .ToList();

                response.Data = new JsonResultPaging<List<CustomerProductOrderStatisticsViewModel>>
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
                _logger.LogError($"GetCustomerProductOrderStatistics: {ex.Message}");
                return response;
            }
        }
    }
}
