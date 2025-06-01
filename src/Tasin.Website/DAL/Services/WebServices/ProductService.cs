using AutoMapper;
using ClosedXML.Excel;
using LinqKit;
using System.Linq.Expressions;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
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
        private readonly IProductRepository _productRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISpecialProductTaxRateRepository _specialProductTaxRateRepository;

        public ProductService(
            ILogger<ProductService> logger,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IUnitRepository unitRepository,
            ICategoryRepository categoryRepository,
            ISpecialProductTaxRateRepository specialProductTaxRateRepository,
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
            _specialProductTaxRateRepository = specialProductTaxRateRepository;
        }

        #region Helper Methods

        /// <summary>
        /// Loads related entity names for a collection of products
        /// </summary>
        private async Task LoadRelatedEntityNamesAsync(List<ProductViewModel> products)
        {
            if (!products.Any()) return;

            // Get distinct IDs for batch loading
            var unitIds = products.Where(p => p.Unit_ID.HasValue).Select(p => p.Unit_ID.Value).Distinct().ToList();
            var categoryIds = products.Where(p => p.Category_ID.HasValue).Select(p => p.Category_ID.Value).Distinct().ToList();
            var specialProductTaxRateIds = products.Where(p => p.SpecialProductTaxRate_ID.HasValue).Select(p => p.SpecialProductTaxRate_ID.Value).Distinct().ToList();
            var parentIds = products.Where(p => p.ParentID.HasValue).Select(p => p.ParentID.Value).Distinct().ToList();

            // Load related entities sequentially to avoid DbContext threading issues
            IEnumerable<Unit> units = new List<Unit>();
            if (unitIds.Count > 0)
            {
                units = await _unitRepository.ReadOnlyRespository.GetAsync(i => unitIds.Contains(i.ID));
            }

            IEnumerable<Category> categories = new List<Category>();
            if (categoryIds.Count > 0)
            {
                categories = await _categoryRepository.ReadOnlyRespository.GetAsync(i => categoryIds.Contains(i.ID));
            }

            IEnumerable<SpecialProductTaxRate> specialProductTaxRates = new List<SpecialProductTaxRate>();
            if (specialProductTaxRateIds.Count > 0)
            {
                specialProductTaxRates = await _specialProductTaxRateRepository.ReadOnlyRespository.GetAsync(i => specialProductTaxRateIds.Contains(i.ID));
            }

            IEnumerable<Product> parentProducts = new List<Product>();
            if (parentIds.Count > 0)
            {
                parentProducts = await _productRepository.ReadOnlyRespository.GetAsync(i => parentIds.Contains(i.ID));
            }

            // Create lookup dictionaries for O(1) access
            var unitLookup = units.ToDictionary(u => u.ID, u => u.Name);
            var categoryLookup = categories.ToDictionary(c => c.ID, c => c.Name);
            var specialProductTaxRateLookup = specialProductTaxRates.ToDictionary(s => s.ID, s => s.Name);
            var parentProductLookup = parentProducts.ToDictionary(p => p.ID, p => p.Name);

            // Assign names using lookups
            foreach (var product in products)
            {
                if (product.Unit_ID.HasValue && unitLookup.TryGetValue(product.Unit_ID.Value, out var unitName))
                    product.UnitName = unitName;

                if (product.Category_ID.HasValue && categoryLookup.TryGetValue(product.Category_ID.Value, out var categoryName))
                    product.CategoryName = categoryName;

                // ProcessingTypeName is now computed from enum in ProductViewModel

                if (product.SpecialProductTaxRate_ID.HasValue && specialProductTaxRateLookup.TryGetValue(product.SpecialProductTaxRate_ID.Value, out var specialProductTaxRateName))
                    product.SpecialProductTaxRateName = specialProductTaxRateName;

                if (product.ParentID.HasValue && parentProductLookup.TryGetValue(product.ParentID.Value, out var parentName))
                    product.ParentName = parentName;
            }
        }

        /// <summary>
        /// Loads user names for a collection of view models
        /// </summary>
        private async Task LoadUserNamesAsync<T>(List<T> viewModels) where T : BaseViewModel
        {
            if (viewModels.Count == 0) return;

            var userIdList = viewModels
                .SelectMany(i => new[] { i.CreatedBy, i.UpdatedBy ?? 0 })
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (userIdList.Count == 0) return;

            var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));
            var userLookup = userList.ToDictionary(u => u.Id, u => u.Name);

            foreach (var viewModel in viewModels)
            {
                var updatedByName = viewModel.UpdatedBy.HasValue && viewModel.UpdatedBy > 0 && userLookup.TryGetValue(viewModel.UpdatedBy.Value, out var updatedName) ? updatedName : null;
                var createdByName = userLookup.TryGetValue(viewModel.CreatedBy, out var createdName) ? createdName : null;
                viewModel.UpdatedByName = updatedByName ?? createdByName ?? "";
            }
        }

        /// <summary>
        /// Validates product data
        /// </summary>
        private async Task<Acknowledgement> ValidateProductDataAsync(ProductViewModel productData)
        {
            var ack = new Acknowledgement { IsSuccess = true };

            if (string.IsNullOrWhiteSpace(productData.Name))
            {
                ack.AddMessage("Tên sản phẩm không được để trống.");
                ack.IsSuccess = false;
            }

            // Business rule: If ParentID is null, then ProcessingType must be Material
            // If ParentID has value, then ProcessingType must not be Material (child products cannot be materials)
            if (!productData.ParentID.HasValue && productData.ProcessingType != EProcessingType.Material)
            {
                ack.AddMessage("Sản phẩm gốc (không có sản phẩm cha) phải là nguyên liệu.");
                ack.IsSuccess = false;
            }
            else if (productData.ParentID.HasValue && productData.ProcessingType == EProcessingType.Material)
            {
                ack.AddMessage("Sản phẩm con (có sản phẩm cha) không thể là nguyên liệu.");
                ack.IsSuccess = false;
            }

            // If ParentID is specified, validate that the parent product exists and is a material
            if (productData.ParentID.HasValue)
            {
                // Prevent circular reference
                if (productData.Id != 0 && productData.Id == productData.ParentID)
                {
                    ack.AddMessage("Sản phẩm không thể là sản phẩm cha của chính nó.");
                    ack.IsSuccess = false;
                }
                else
                {
                    var parentProduct = await _productRepository.ReadOnlyRespository.FindAsync(productData.ParentID.Value);
                    if (parentProduct == null)
                    {
                        ack.AddMessage("Sản phẩm cha không tồn tại.");
                        ack.IsSuccess = false;
                    }
                    else if (parentProduct.ProcessingType != EProcessingType.Material)
                    {
                        ack.AddMessage("Sản phẩm cha phải là nguyên liệu.");
                        ack.IsSuccess = false;
                    }
                    else if (!parentProduct.IsActive)
                    {
                        ack.AddMessage("Sản phẩm cha đã bị vô hiệu hóa.");
                        ack.IsSuccess = false;
                    }
                }
            }

            return ack;
        }

        /// <summary>
        /// Creates a new product entity from view model
        /// </summary>
        private async Task<Product> CreateProductEntityAsync(ProductViewModel productData)
        {
            var product = _mapper.Map<Product>(productData);
            product.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Product, DbContext);
            product.NameNonUnicode = Utils.NonUnicode(product.Name);
            product.CreatedDate = DateTime.Now;
            product.CreatedBy = CurrentUserId;
            product.UpdatedDate = product.CreatedDate;
            product.UpdatedBy = product.CreatedBy;
            product.IsActive = true;
            product.Status = ECommonStatus.Actived.ToString();
            return product;
        }

        /// <summary>
        /// Updates an existing product entity from view model
        /// </summary>
        private static void UpdateProductEntity(Product existingProduct, ProductViewModel productData, int currentUserId)
        {
            existingProduct.Name = productData.Name;
            existingProduct.NameNonUnicode = Utils.NonUnicode(productData.Name);
            existingProduct.Name_EN = productData.Name_EN;
            existingProduct.Unit_ID = productData.Unit_ID;
            existingProduct.Category_ID = productData.Category_ID;
            existingProduct.ProcessingType = productData.ProcessingType;
            existingProduct.LossRate = productData.LossRate;
            existingProduct.AdditionalCost = productData.AdditionalCost;
            existingProduct.Note = productData.Note;
            existingProduct.IsDiscontinued = productData.IsDiscontinued;
            existingProduct.ProcessingFee = productData.ProcessingFee;
            existingProduct.CompanyTaxRate = productData.CompanyTaxRate;
            existingProduct.ConsumerTaxRate = productData.ConsumerTaxRate;
            existingProduct.SpecialProductTaxRate_ID = productData.SpecialProductTaxRate_ID;
            existingProduct.ParentID = productData.ParentID;
            existingProduct.UpdatedDate = DateTime.Now;
            existingProduct.UpdatedBy = currentUserId;
        }

        #endregion

        #region Public Methods

        public async Task<Acknowledgement<JsonResultPaging<List<ProductViewModel>>>> GetProductList(ProductSearchModel searchModel)
        {
            return await GetProductList(searchModel, null, null);
        }

        public async Task<Acknowledgement<JsonResultPaging<List<ProductViewModel>>>> GetProductList(ProductSearchModel searchModel, Expression<Func<Product, object>>? selector = null, int? excludeProductId = null)
        {
            var response = new Acknowledgement<JsonResultPaging<List<ProductViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Product>(i => i.IsActive == true);

                if (!searchModel.IncludeDiscontinued)
                {
                    predicate = predicate.And(i => !i.IsDiscontinued);
                }

                // Exclude specific product ID if provided (for parent dropdown)
                if (excludeProductId.HasValue)
                {
                    predicate = predicate.And(i => i.ID != excludeProductId.Value);
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

                if (searchModel.ProcessingType.HasValue)
                {
                    predicate = predicate.And(i => i.ProcessingType == searchModel.ProcessingType.Value);
                }

                // Add author predicate if needed
                predicate = ProductAuthorPredicate.GetProductAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var productQuery = await _productRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var productViewModels = _mapper.Map<List<ProductViewModel>>(productQuery.Data);

                // Load related entity names and user names using optimized helper methods
                await LoadRelatedEntityNamesAsync(productViewModels);
                await LoadUserNamesAsync(productViewModels);

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

                // Load related entity names and user names using optimized helper methods
                await LoadRelatedEntityNamesAsync(new List<ProductViewModel> { productViewModel });
                await LoadUserNamesAsync(new List<ProductViewModel> { productViewModel });

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
            return postData.Id == 0
                ? await CreateProduct(postData)
                : await UpdateProduct(postData);
        }

        public async Task<Acknowledgement> CreateProduct(ProductViewModel postData)
        {
            var ack = await ValidateProductDataAsync(postData);
            if (!ack.IsSuccess) return ack;

            try
            {
                var newProduct = await CreateProductEntityAsync(postData);
                await ack.TrySaveChangesAsync(res => res.AddAsync(newProduct), _productRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateProduct: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> UpdateProduct(ProductViewModel postData)
        {
            var ack = await ValidateProductDataAsync(postData);
            if (!ack.IsSuccess) return ack;

            try
            {
                var existingProduct = await _productRepository.Repository.FindAsync(postData.Id);
                if (existingProduct == null)
                {
                    ack.AddMessage("Không tìm thấy sản phẩm.");
                    return ack;
                }

                // Check if trying to discontinue a product that has active child products
                if (postData.IsDiscontinued && !existingProduct.IsDiscontinued)
                {
                    var activeChildProducts = await _productRepository.ReadOnlyRespository.GetAsync(p => p.ParentID == postData.Id && p.IsActive && !p.IsDiscontinued);
                    if (activeChildProducts.Count > 0)
                    {
                        ack.AddMessage("Không thể ngừng sử dụng sản phẩm này vì có sản phẩm con đang hoạt động.");
                        return ack;
                    }
                }

                // Check if trying to change ParentID when product has child products
                if (existingProduct.ParentID != postData.ParentID)
                {
                    var childProducts = await _productRepository.ReadOnlyRespository.GetAsync(p => p.ParentID == postData.Id && p.IsActive);
                    if (childProducts.Count > 0)
                    {
                        ack.AddMessage("Không thể thay đổi sản phẩm cha khi sản phẩm này đã có sản phẩm con.");
                        return ack;
                    }
                }

                UpdateProductEntity(existingProduct, postData, CurrentUserId);
                await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingProduct), _productRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"UpdateProduct: {ex.Message}");
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

                // Check if product has child products
                var childProducts = await _productRepository.ReadOnlyRespository.GetAsync(p => p.ParentID == productId && p.IsActive);
                if (childProducts.Count > 0)
                {
                    ack.AddMessage("Không thể xóa sản phẩm này vì có sản phẩm con đang sử dụng.");
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

        public async Task<Acknowledgement<ProductExcelImportResult>> ImportProductsFromExcel(IFormFile file)
        {
            var response = new Acknowledgement<ProductExcelImportResult>();
            var result = new ProductExcelImportResult();

            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    response.AddMessage("Vui lòng chọn file Excel để import.");
                    return response;
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!FileHelper.ValidateFileExt(EFileType.Excel, fileExtension))
                {
                    response.AddMessage("File không đúng định dạng Excel (.xlsx, .xls, .xlsm, .csv).");
                    return response;
                }

                // Read Excel file
                var importModels = new List<ProductExcelImportModel>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = ExcelHelper.CreateWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row

                        int rowNumber = 2; // Start from row 2 (after header)
                        foreach (var row in rows)
                        {
                            var importModel = new ProductExcelImportModel
                            {
                                RowNumber = rowNumber,
                                Name = ExcelHelper.GetCellStringValue(row.Cell(1)),
                                Name_EN = ExcelHelper.GetCellStringValue(row.Cell(2)),
                                UnitCode = ExcelHelper.GetCellStringValue(row.Cell(3)),
                                CategoryCode = ExcelHelper.GetCellStringValue(row.Cell(4)),
                                ProcessingTypeText = ExcelHelper.GetCellStringValue(row.Cell(5)),
                                SpecialProductTaxRateCode = ExcelHelper.GetCellStringValue(row.Cell(6)),
                                LossRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(7))),
                                AdditionalCost = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(8))),
                                ProcessingFee = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(9))),
                                DefaultPrice = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(10))),
                                CompanyTaxRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(11))),
                                ConsumerTaxRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(12))),
                                Note = ExcelHelper.GetCellStringValue(row.Cell(13)),
                                IsDiscontinuedText = ExcelHelper.GetCellStringValue(row.Cell(14))
                            };

                            // Validate required fields
                            if (string.IsNullOrEmpty(importModel.Name))
                            {
                                importModel.ValidationErrors.Add("Tên sản phẩm không được để trống");
                            }

                            // Business rule validation: Since Excel import doesn't support ParentID,
                            // all imported products are root products and must be materials
                            if (importModel.ProcessingType != EProcessingType.Material)
                            {
                                importModel.ValidationErrors.Add("Sản phẩm gốc (không có sản phẩm cha) phải là nguyên liệu");
                            }

                            importModels.Add(importModel);
                            rowNumber++;
                        }
                    }
                }

                result.TotalRows = importModels.Count;

                // Get lookup data
                var units = await _unitRepository.ReadOnlyRespository.GetAsync(u => u.IsActive);
                var categories = await _categoryRepository.ReadOnlyRespository.GetAsync(c => c.IsActive);
                var specialProductTaxRates = await _specialProductTaxRateRepository.ReadOnlyRespository.GetAsync(s => s.IsActive);

                // Process each row
                foreach (var importModel in importModels)
                {
                    try
                    {
                        // Validate and map lookup values
                        int? unitId = null;
                        if (!string.IsNullOrEmpty(importModel.UnitCode))
                        {
                            var unit = units.FirstOrDefault(u => u.Code.Equals(importModel.UnitCode, StringComparison.OrdinalIgnoreCase));
                            if (unit == null)
                            {
                                importModel.ValidationErrors.Add($"Không tìm thấy đơn vị với mã: {importModel.UnitCode}");
                            }
                            else
                            {
                                unitId = unit.ID;
                            }
                        }

                        int? categoryId = null;
                        if (!string.IsNullOrEmpty(importModel.CategoryCode))
                        {
                            var category = categories.FirstOrDefault(c => c.Code.Equals(importModel.CategoryCode, StringComparison.OrdinalIgnoreCase));
                            if (category == null)
                            {
                                importModel.ValidationErrors.Add($"Không tìm thấy danh mục với mã: {importModel.CategoryCode}");
                            }
                            else
                            {
                                categoryId = category.ID;
                            }
                        }

                        // ProcessingType is now parsed from text in the model

                        int? specialProductTaxRateId = null;
                        if (!string.IsNullOrEmpty(importModel.SpecialProductTaxRateCode))
                        {
                            var specialProductTaxRate = specialProductTaxRates.FirstOrDefault(s => s.Code.Equals(importModel.SpecialProductTaxRateCode, StringComparison.OrdinalIgnoreCase));
                            if (specialProductTaxRate == null)
                            {
                                importModel.ValidationErrors.Add($"Không tìm thấy thuế suất đặc biệt với mã: {importModel.SpecialProductTaxRateCode}");
                            }
                            else
                            {
                                specialProductTaxRateId = specialProductTaxRate.ID;
                            }
                        }

                        // If there are validation errors, add to failed rows
                        if (importModel.ValidationErrors.Any())
                        {
                            result.FailedRows++;
                            result.Errors.Add(new ProductExcelImportError
                            {
                                RowNumber = importModel.RowNumber,
                                ProductName = importModel.Name,
                                ErrorMessages = importModel.ValidationErrors
                            });
                            continue;
                        }

                        // Create product
                        var product = new Product
                        {
                            Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Product, DbContext),
                            Name = importModel.Name,
                            NameNonUnicode = Utils.NonUnicode(importModel.Name),
                            Name_EN = string.IsNullOrEmpty(importModel.Name_EN) ? null : importModel.Name_EN,
                            Unit_ID = unitId,
                            Category_ID = categoryId,
                            ProcessingType = importModel.ProcessingType,
                            ParentID = null, // Excel import only supports root products
                            SpecialProductTaxRate_ID = specialProductTaxRateId,
                            LossRate = importModel.LossRate,
                            AdditionalCost = importModel.AdditionalCost,
                            ProcessingFee = importModel.ProcessingFee,
                            DefaultPrice = importModel.DefaultPrice,
                            CompanyTaxRate = importModel.CompanyTaxRate,
                            ConsumerTaxRate = importModel.ConsumerTaxRate,
                            Note = string.IsNullOrEmpty(importModel.Note) ? null : importModel.Note,
                            IsDiscontinued = importModel.IsDiscontinued,
                            CreatedDate = DateTime.Now,
                            CreatedBy = CurrentUserId,
                            UpdatedDate = DateTime.Now,
                            UpdatedBy = CurrentUserId,
                            IsActive = true,
                            Status = ECommonStatus.Actived.ToString()
                        };

                        await _productRepository.Repository.AddAsync(product);
                        result.SuccessfulRows++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedRows++;
                        result.Errors.Add(new ProductExcelImportError
                        {
                            RowNumber = importModel.RowNumber,
                            ProductName = importModel.Name,
                            ErrorMessages = new List<string> { $"Lỗi xử lý: {ex.Message}" }
                        });
                        _logger.LogError($"Error processing row {importModel.RowNumber}: {ex.Message}");
                    }
                }

                // Save changes if there are successful rows
                if (result.SuccessfulRows > 0)
                {
                    await DbContext.SaveChangesAsync();
                }

                response.Data = result;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"ImportProductsFromExcel: {ex.Message}");
                return response;
            }
        }

        public Task<byte[]> GenerateProductExcelTemplate()
        {
            try
            {
                using (var workbook = ExcelHelper.CreateWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Products");

                    // Set headers
                    var headers = new[]
                    {
                        "Tên sản phẩm (*)",
                        "Tên tiếng Anh",
                        "Mã đơn vị",
                        "Mã quy cách",
                        "Loại chế biến (*)",
                        "Mã thuế suất đặc biệt",
                        "Tỷ lệ hao hụt (%)",
                        "Chi phí thêm",
                        "Phí chế biến",
                        "Đơn giá mặc định",
                        "Thuế suất công ty (%) - Tùy chọn",
                        "Thuế suất người tiêu dùng (%) - Tùy chọn",
                        "Ghi chú",
                        "Ngừng sản xuất (Y/N)"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cell(1, i + 1);
                        ExcelHelper.SetCellValue(cell, headers[i]);
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    // Add sample data
                    ExcelHelper.SetCellValue(worksheet.Cell(2, 1), "Sản phẩm mẫu 1");
                    ExcelHelper.SetCellValue(worksheet.Cell(2, 2), "Sample Product 1");
                    worksheet.Cell(2, 3).Value = "KG";
                    worksheet.Cell(2, 4).Value = "CAT001";
                    worksheet.Cell(2, 5).Value = "Material";
                    worksheet.Cell(2, 6).Value = "SPTR001";
                    worksheet.Cell(2, 7).Value = 5;
                    worksheet.Cell(2, 8).Value = 2000;
                    worksheet.Cell(2, 9).Value = 1000;
                    worksheet.Cell(2, 10).Value = 50000;
                    worksheet.Cell(2, 11).Value = 8;
                    worksheet.Cell(2, 12).Value = 10;
                    worksheet.Cell(2, 13).Value = "Ghi chú mẫu";
                    worksheet.Cell(2, 14).Value = "N";

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Add instructions sheet
                    var instructionSheet = workbook.Worksheets.Add("Hướng dẫn");
                    instructionSheet.Cell(1, 1).Value = "HƯỚNG DẪN IMPORT SẢN PHẨM";
                    instructionSheet.Cell(1, 1).Style.Font.Bold = true;
                    instructionSheet.Cell(1, 1).Style.Font.FontSize = 16;

                    var instructions = new[]
                    {
                        "",
                        "1. Các cột bắt buộc:",
                        "   - Tên sản phẩm (*): Không được để trống",
                        "",
                        "2. Các cột tùy chọn:",
                        "   - Tên tiếng Anh: Có thể để trống",
                        "   - Mã đơn vị: Phải tồn tại trong hệ thống",
                        "   - Mã quy cách: Phải tồn tại trong hệ thống",
                        "   - Loại chế biến (*): Nhập 'Material', 'SemiProcessed', hoặc 'FinishedProduct' (BẮT BUỘC phải là 'Material' cho sản phẩm gốc)",
                        "   - Mã thuế suất đặc biệt: Phải tồn tại trong hệ thống",
                        "   - Tỷ lệ hao hụt (%): Nhập số thập phân (ví dụ: 5.5)",
                        "   - Chi phí thêm: Nhập số (ví dụ: 2000)",
                        "   - Phí chế biến: Nhập số (ví dụ: 1000)",
                        "   - Đơn giá mặc định: Nhập số (ví dụ: 50000)",
                        "   - Thuế suất công ty (%): Nhập số thập phân (ví dụ: 10.5) hoặc để trống",
                        "   - Thuế suất người tiêu dùng (%): Nhập số thập phân (ví dụ: 8.0) hoặc để trống",
                        "   - Ghi chú: Có thể để trống",
                        "   - Ngừng sản xuất: Nhập Y/N, Yes/No, True/False, 1/0",
                        "",
                        "3. Quy tắc nghiệp vụ:",
                        "   - Tất cả sản phẩm import từ Excel đều là sản phẩm gốc (không có sản phẩm cha)",
                        "   - Sản phẩm gốc BẮT BUỘC phải có loại chế biến là 'Material'",
                        "",
                        "4. Lưu ý:",
                        "   - Không được xóa dòng tiêu đề",
                        "   - Dữ liệu bắt đầu từ dòng 2",
                        "   - File phải có định dạng Excel (.xlsx, .xls, .xlsm) hoặc CSV",
                        "   - Mã các thực thể liên quan phải tồn tại trong hệ thống"
                    };

                    for (int i = 0; i < instructions.Length; i++)
                    {
                        ExcelHelper.SetCellValue(instructionSheet.Cell(i + 2, 1), instructions[i]);
                    }

                    instructionSheet.Columns().AdjustToContents();

                    return Task.FromResult(ExcelHelper.SaveToByteArray(workbook));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GenerateProductExcelTemplate: {ex.Message}");
                throw;
            }
        }



        private static decimal? ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value.Trim(), out decimal result))
                return result;

            return null;
        }

        #endregion
    }
}
