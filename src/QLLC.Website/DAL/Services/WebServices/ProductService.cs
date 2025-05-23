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
    public class ProductService : BaseService<ProductService>, IProductService
    {
        private readonly IMapper _mapper;
        private IProductRepository _productRepository;
        private IUnitRepository _unitRepository;
        private ICategoryRepository _categoryRepository;
        private IProcessingTypeRepository _processingTypeRepository;
        private IMaterialRepository _materialRepository;

        public ProductService(
            ILogger<ProductService> logger,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IUnitRepository unitRepository,
            ICategoryRepository categoryRepository,
            IProcessingTypeRepository processingTypeRepository,
            IMaterialRepository materialRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _productRepository = productRepository;
            _unitRepository = unitRepository;
            _categoryRepository = categoryRepository;
            _processingTypeRepository = processingTypeRepository;
            _materialRepository = materialRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<ProductViewModel>>>> GetProductList(ProductSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<ProductViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Product>(i => i.IsActive == true);

                if (!searchModel.IncludeDiscontinued)
                {
                    predicate = predicate.And(i => !i.IsDiscontinued);
                }

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => 
                        (i.NameNonUnicode != null && i.NameNonUnicode.ToLower().Contains(searchStringNonUnicode)) ||
                        i.Code.ToLower().Contains(searchStringNonUnicode)
                    );
                }

                if (searchModel.Unit_ID.HasValue)
                {
                    predicate = predicate.And(i => i.Unit_ID == searchModel.Unit_ID);
                }

                if (searchModel.Category_ID.HasValue)
                {
                    predicate = predicate.And(i => i.Category_ID == searchModel.Category_ID);
                }

                if (searchModel.ProcessingType_ID.HasValue)
                {
                    predicate = predicate.And(i => i.ProcessingType_ID == searchModel.ProcessingType_ID);
                }

                if (searchModel.Material_ID.HasValue)
                {
                    predicate = predicate.And(i => i.Material_ID == searchModel.Material_ID);
                }

                // Add author predicate if needed
                predicate = ProductAuthorPredicate.GetProductAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var productQuery = await _productRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var productViewModels = _mapper.Map<List<ProductViewModel>>(productQuery.Data);

                // Add related entity names
                var unitIds = productViewModels.Where(p => p.Unit_ID.HasValue).Select(p => p.Unit_ID.Value).Distinct().ToList();
                var categoryIds = productViewModels.Where(p => p.Category_ID.HasValue).Select(p => p.Category_ID.Value).Distinct().ToList();
                var processingTypeIds = productViewModels.Where(p => p.ProcessingType_ID.HasValue).Select(p => p.ProcessingType_ID.Value).Distinct().ToList();
                var materialIds = productViewModels.Where(p => p.Material_ID.HasValue).Select(p => p.Material_ID.Value).Distinct().ToList();

                var units = await _unitRepository.ReadOnlyRespository.GetAsync(i => unitIds.Contains(i.ID));
                var categories = await _categoryRepository.ReadOnlyRespository.GetAsync(i => categoryIds.Contains(i.ID));
                var processingTypes = await _processingTypeRepository.ReadOnlyRespository.GetAsync(i => processingTypeIds.Contains(i.ID));
                var materials = await _materialRepository.ReadOnlyRespository.GetAsync(i => materialIds.Contains(i.ID));

                foreach (var product in productViewModels)
                {
                    if (product.Unit_ID.HasValue)
                    {
                        var unit = units.FirstOrDefault(u => u.ID == product.Unit_ID.Value);
                        product.UnitName = unit?.Name;
                    }

                    if (product.Category_ID.HasValue)
                    {
                        var category = categories.FirstOrDefault(c => c.ID == product.Category_ID.Value);
                        product.CategoryName = category?.Name;
                    }

                    if (product.ProcessingType_ID.HasValue)
                    {
                        var processingType = processingTypes.FirstOrDefault(p => p.ID == product.ProcessingType_ID.Value);
                        product.ProcessingTypeName = processingType?.Name;
                    }

                    if (product.Material_ID.HasValue)
                    {
                        var material = materials.FirstOrDefault(m => m.ID == product.Material_ID.Value);
                        product.MaterialName = material?.Name;
                    }
                }

                // Add user names
                var userIdList = productViewModels
                    .SelectMany(i => new[] { i.CreatedBy, i.UpdatedBy })
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                foreach (var product in productViewModels)
                {
                    var createdByUser = userList.FirstOrDefault(i => i.Id == product.CreatedBy);
                    var updatedByUser = userList.FirstOrDefault(i => i.Id == product.UpdatedBy);

                    product.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "";
                }

                response.Data = new JsonResultPaging<List<ProductViewModel>>
                {
                    Data = productViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = productQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetProductList: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<ProductViewModel>> GetProductById(int productId)
        {
            var response = new Acknowledgement<ProductViewModel>();
            try
            {
                var product = await _productRepository.ReadOnlyRespository.FindAsync(productId);
                if (product == null)
                {
                    response.AddMessage("Không tìm thấy sản phẩm.");
                    return response;
                }

                var productViewModel = _mapper.Map<ProductViewModel>(product);

                // Add related entity names
                if (product.Unit_ID.HasValue)
                {
                    var unit = await _unitRepository.ReadOnlyRespository.FindAsync(product.Unit_ID.Value);
                    productViewModel.UnitName = unit?.Name;
                }

                if (product.Category_ID.HasValue)
                {
                    var category = await _categoryRepository.ReadOnlyRespository.FindAsync(product.Category_ID.Value);
                    productViewModel.CategoryName = category?.Name;
                }

                if (product.ProcessingType_ID.HasValue)
                {
                    var processingType = await _processingTypeRepository.ReadOnlyRespository.FindAsync(product.ProcessingType_ID.Value);
                    productViewModel.ProcessingTypeName = processingType?.Name;
                }

                if (product.Material_ID.HasValue)
                {
                    var material = await _materialRepository.ReadOnlyRespository.FindAsync(product.Material_ID.Value);
                    productViewModel.MaterialName = material?.Name;
                }

                // Add user names
                var createdByUser = await _userRepository.ReadOnlyRespository.FindAsync(product.CreatedBy);
                var updatedByUser = product.UpdatedBy > 0 ? await _userRepository.ReadOnlyRespository.FindAsync(product.UpdatedBy) : null;

                productViewModel.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "";

                response.Data = productViewModel;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetProductById: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateProduct(ProductViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(postData.Name))
                {
                    ack.AddMessage("Tên sản phẩm không được để trống.");
                    return ack;
                }

                if (postData.Id == 0)
                {
                    var newProduct = _mapper.Map<Product>(postData);
                    newProduct.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Product, DbContext);
                    newProduct.NameNonUnicode = Utils.NonUnicode(newProduct.Name);
                    newProduct.CreatedDate = DateTime.Now;
                    newProduct.CreatedBy = CurrentUserId;
                    newProduct.UpdatedDate = newProduct.CreatedDate;
                    newProduct.UpdatedBy = newProduct.CreatedBy;
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newProduct), _productRepository.Repository);
                }
                else
                {
                    var existingProduct = await _productRepository.Repository.FindAsync(postData.Id);
                    if (existingProduct == null)
                    {
                        ack.AddMessage("Không tìm thấy sản phẩm.");
                        return ack;
                    }

                    existingProduct.Name = postData.Name;
                    existingProduct.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existingProduct.Name_EN = postData.Name_EN;
                    existingProduct.Unit_ID = postData.Unit_ID;
                    existingProduct.Category_ID = postData.Category_ID;
                    existingProduct.ProcessingType_ID = postData.ProcessingType_ID;
                    existingProduct.TaxRate = postData.TaxRate;
                    existingProduct.TaxRateConfig_ID = postData.TaxRateConfig_ID;
                    existingProduct.LossRate = postData.LossRate;
                    existingProduct.Material_ID = postData.Material_ID;
                    existingProduct.ProfitMargin = postData.ProfitMargin;
                    existingProduct.Note = postData.Note;
                    existingProduct.IsDiscontinued = postData.IsDiscontinued;
                    existingProduct.ProcessingFee = postData.ProcessingFee;
                    existingProduct.UpdatedDate = DateTime.Now;
                    existingProduct.UpdatedBy = CurrentUserId;
                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingProduct), _productRepository.Repository);
                }
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateOrUpdateProduct: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteProductById(int productId)
        {
            var ack = new Acknowledgement();
            try
            {
                var product = await _productRepository.Repository.FindAsync(productId);
                if (product == null)
                {
                    ack.AddMessage("Không tìm thấy sản phẩm.");
                    return ack;
                }

                product.IsActive = false;
                product.UpdatedDate = DateTime.Now;
                product.UpdatedBy = CurrentUserId;
                await ack.TrySaveChangesAsync(res => res.UpdateAsync(product), _productRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"DeleteProductById: {ex.Message}");
                return ack;
            }
        }
    }
}
