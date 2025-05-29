using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebServices
{
    public class ProductVendorService : BaseService<ProductVendorService>, IProductVendorService
    {
        private readonly IProduct_VendorRepository _productVendorRepository;
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IMapper _mapper;

        public ProductVendorService(
            ILogger<ProductVendorService> logger,
            IUserRepository userRepository,
            IProduct_VendorRepository productVendorRepository,
            IProductRepository productRepository,
            IVendorRepository vendorRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _productVendorRepository = productVendorRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _mapper = mapper;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<ProductVendorViewModel>>>> GetProductVendorList(ProductVendorSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<ProductVendorViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Product_Vendor>(true);

                if (searchModel.VendorId.HasValue)
                {
                    predicate = predicate.And(pv => pv.Vendor_ID == searchModel.VendorId.Value);
                }

                if (searchModel.ProductId.HasValue)
                {
                    predicate = predicate.And(pv => pv.Product_ID == searchModel.ProductId.Value);
                }

                if (searchModel.MinPrice.HasValue)
                {
                    predicate = predicate.And(pv => pv.Price >= searchModel.MinPrice.Value);
                }

                if (searchModel.MaxPrice.HasValue)
                {
                    predicate = predicate.And(pv => pv.Price <= searchModel.MaxPrice.Value);
                }

                if (searchModel.Priority.HasValue)
                {
                    predicate = predicate.And(pv => pv.Priority == searchModel.Priority.Value);
                }

                var productVendorQuery = await _productVendorRepository.ReadOnlyRespository.GetWithPagingAsync(
                new PagingParameters(searchModel.PageNumber, searchModel.PageSize),
                predicate,
                q => q.OrderBy(pv => pv.Vendor_ID).ThenBy(pv => pv.Priority ?? int.MaxValue),
                "Vendor,Product"
                );

                var productVendorViewModels = _mapper.Map<List<ProductVendorViewModel>>(productVendorQuery.Data);

                // Set display names
                foreach (var item in productVendorViewModels)
                {
                    var productVendor = productVendorQuery.Data.FirstOrDefault(pv => 
                        pv.Vendor_ID == item.Vendor_ID && pv.Product_ID == item.Product_ID);
                    
                    if (productVendor != null)
                    {
                        item.VendorName = productVendor.Vendor?.Name;
                        item.ProductName = productVendor.Product?.Name;
                        item.ProductCode = productVendor.Product?.Code;
                    }
                }

                response.Data = new JsonResultPaging<List<ProductVendorViewModel>>
                {
                    Data = productVendorViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = productVendorQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetProductVendorList: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<List<ProductVendorViewModel>>> GetProductsByVendorId(int vendorId)
        {
            var response = new Acknowledgement<List<ProductVendorViewModel>>();
            try
            {
                var productVendors = await _productVendorRepository.ReadOnlyRespository.GetAsync(
                    pv => pv.Vendor_ID == vendorId,
                    q => q.OrderBy(pv => pv.Priority ?? int.MaxValue),
                    null,
                    "Product", 
                    e => new ProductVendorViewModel
                    {
                        Vendor_ID = e.Vendor_ID,
                        Product_ID = e.Product_ID,
                        Price = e.Price,
                        UnitPrice = e.UnitPrice,
                        Priority = e.Priority,
                        Description = e.Description,
                        VendorName = e.Vendor.Name,
                        ProductName = e.Product.Name,
                        ProductCode = e.Product.Code,

                    }
                );

                response.Data = productVendors;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetProductsByVendorId: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<List<ProductVendorViewModel>>> GetVendorsByProductId(int productId)
        {
            var response = new Acknowledgement<List<ProductVendorViewModel>>();
            try
            {
                var productVendors = await _productVendorRepository.ReadOnlyRespository.GetAsync(
                    pv => pv.Product_ID == productId,
                    q => q.OrderBy(pv => pv.Priority ?? int.MaxValue),
                    null,
                    "Vendor"
                );

                var viewModels = _mapper.Map<List<ProductVendorViewModel>>(productVendors);
                
                // Set display names
                foreach (var item in viewModels)
                {
                    var productVendor = productVendors.FirstOrDefault(pv => 
                        pv.Vendor_ID == item.Vendor_ID && pv.Product_ID == item.Product_ID);
                    
                    if (productVendor?.Vendor != null)
                    {
                        item.VendorName = productVendor.Vendor.Name;
                    }
                }

                response.Data = viewModels;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetVendorsByProductId: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<ProductVendorViewModel>> GetProductVendorById(int vendorId, int productId)
        {
            var response = new Acknowledgement<ProductVendorViewModel>();
            try
            {
                var productVendor = await _productVendorRepository.ReadOnlyRespository.FirstOrDefaultAsync(
                    pv => pv.Vendor_ID == vendorId && pv.Product_ID == productId,
                    "Vendor,Product"
                );

                if (productVendor == null)
                {
                    response.AddMessage("Không tìm thấy mối quan hệ sản phẩm-nhà cung cấp.");
                    return response;
                }

                var viewModel = _mapper.Map<ProductVendorViewModel>(productVendor);
                viewModel.VendorName = productVendor.Vendor?.Name;
                viewModel.ProductName = productVendor.Product?.Name;
                viewModel.ProductCode = productVendor.Product?.Code;

                response.Data = viewModel;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetProductVendorById: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateProductVendor(ProductVendorViewModel model)
        {
            var ack = new Acknowledgement();
            try
            {
                // Validate vendor exists
                var vendor = await _vendorRepository.Repository.FindAsync(model.Vendor_ID);
                if (vendor == null)
                {
                    ack.AddMessage("Không tìm thấy nhà cung cấp.");
                    return ack;
                }

                // Validate product exists
                var product = await _productRepository.Repository.FindAsync(model.Product_ID);
                if (product == null)
                {
                    ack.AddMessage("Không tìm thấy sản phẩm.");
                    return ack;
                }

                // Check if relationship already exists
                var existingProductVendor = await _productVendorRepository.Repository
                    .FirstOrDefaultAsync(pv => pv.Vendor_ID == model.Vendor_ID && pv.Product_ID == model.Product_ID);

                if (existingProductVendor != null)
                {
                    // Update existing relationship
                    existingProductVendor.Price = model.Price;
                    existingProductVendor.UnitPrice = model.UnitPrice;
                    existingProductVendor.Priority = model.Priority;
                    existingProductVendor.Description = model.Description;

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingProductVendor), _productVendorRepository.Repository);
                }
                else
                {
                    // Create new relationship
                    var newProductVendor = _mapper.Map<Product_Vendor>(model);
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newProductVendor), _productVendorRepository.Repository);
                }

                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateOrUpdateProductVendor: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> BulkAddProductsToVendor(BulkProductVendorViewModel model)
        {
            var ack = new Acknowledgement();
            try
            {
                // Validate vendor exists
                var vendor = await _vendorRepository.Repository.FindAsync(model.VendorId);
                if (vendor == null)
                {
                    ack.AddMessage("Không tìm thấy nhà cung cấp.");
                    return ack;
                }

                if (model.Products == null || !model.Products.Any())
                {
                    ack.AddMessage("Danh sách sản phẩm không được để trống.");
                    return ack;
                }

                var productIds = model.Products.Select(p => p.Product_ID).ToList();
                var products = await _productRepository.ReadOnlyRespository.GetAsync(p => productIds.Contains(p.ID));

                if (products.Count != productIds.Count)
                {
                    ack.AddMessage("Một số sản phẩm không tồn tại trong hệ thống.");
                    return ack;
                }

                // Get existing relationships to avoid duplicates
                var existingRelationships = await _productVendorRepository.ReadOnlyRespository.GetAsync(
                    pv => pv.Vendor_ID == model.VendorId && productIds.Contains(pv.Product_ID));

                var existingProductIds = existingRelationships.Select(pv => pv.Product_ID).ToHashSet();

                var newProductVendors = new List<Product_Vendor>();
                var updateProductVendors = new List<Product_Vendor>();

                foreach (var productItem in model.Products)
                {
                    if (existingProductIds.Contains(productItem.Product_ID))
                    {
                        // Update existing relationship
                        var existing = existingRelationships.First(pv => pv.Product_ID == productItem.Product_ID);
                        existing.Price = productItem.Price;
                        existing.UnitPrice = productItem.UnitPrice;
                        existing.Priority = productItem.Priority;
                        existing.Description = productItem.Description;
                        updateProductVendors.Add(existing);
                    }
                    else
                    {
                        // Create new relationship
                        var newProductVendor = new Product_Vendor
                        {
                            Vendor_ID = model.VendorId,
                            Product_ID = productItem.Product_ID,
                            Price = productItem.Price,
                            UnitPrice = productItem.UnitPrice,
                            Priority = productItem.Priority,
                            Description = productItem.Description
                        };
                        newProductVendors.Add(newProductVendor);
                    }
                }

                // Save changes
                if (newProductVendors.Any())
                {
                    await _productVendorRepository.Repository.AddRangeAsync(newProductVendors);
                }

                if (updateProductVendors.Any())
                {
                   await _productVendorRepository.Repository.UpdateRangeAsync(updateProductVendors);
                }

                await _productVendorRepository.Repository.SaveChangesAsync();
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"BulkAddProductsToVendor: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteProductVendor(int vendorId, int productId)
        {
            var ack = new Acknowledgement();
            try
            {
                var productVendor = await _productVendorRepository.Repository
                    .FirstOrDefaultAsync(pv => pv.Vendor_ID == vendorId && pv.Product_ID == productId);

                if (productVendor == null)
                {
                    ack.AddMessage("Không tìm thấy mối quan hệ sản phẩm-nhà cung cấp.");
                    return ack;
                }

                await ack.TrySaveChangesAsync(res => res.DeleteAsync(productVendor), _productVendorRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"DeleteProductVendor: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> BulkDeleteProductsFromVendor(int vendorId, List<int> productIds)
        {
            var ack = new Acknowledgement();
            try
            {
                if (productIds == null || !productIds.Any())
                {
                    ack.AddMessage("Danh sách sản phẩm không được để trống.");
                    return ack;
                }

                var productVendors = await _productVendorRepository.ReadOnlyRespository.GetAsync(
                    pv => pv.Vendor_ID == vendorId && productIds.Contains(pv.Product_ID));

                if (!productVendors.Any())
                {
                    ack.AddMessage("Không tìm thấy mối quan hệ sản phẩm-nhà cung cấp nào để xóa.");
                    return ack;
                }

                await _productVendorRepository.Repository.DeleteRangeAsync(productVendors);
                await _productVendorRepository.Repository.SaveChangesAsync();

                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"BulkDeleteProductsFromVendor: {ex.Message}");
                return ack;
            }
        }

       
    }
}
