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
        private readonly IProcessingTypeRepository _processingTypeRepository;
        private readonly ISpecialProductTaxRateRepository _specialProductTaxRateRepository;

        public ProductService(
            ILogger<ProductService> logger,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IUnitRepository unitRepository,
            ICategoryRepository categoryRepository,
            IProcessingTypeRepository processingTypeRepository,
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
            _processingTypeRepository = processingTypeRepository;
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
            var processingTypeIds = products.Where(p => p.ProcessingType_ID.HasValue).Select(p => p.ProcessingType_ID.Value).Distinct().ToList();
            var specialProductTaxRateIds = products.Where(p => p.SpecialProductTaxRate_ID.HasValue).Select(p => p.SpecialProductTaxRate_ID.Value).Distinct().ToList();

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

            IEnumerable<ProcessingType> processingTypes = new List<ProcessingType>();
            if (processingTypeIds.Count > 0)
            {
                processingTypes = await _processingTypeRepository.ReadOnlyRespository.GetAsync(i => processingTypeIds.Contains(i.ID));
            }

            IEnumerable<SpecialProductTaxRate> specialProductTaxRates = new List<SpecialProductTaxRate>();
            if (specialProductTaxRateIds.Count > 0)
            {
                specialProductTaxRates = await _specialProductTaxRateRepository.ReadOnlyRespository.GetAsync(i => specialProductTaxRateIds.Contains(i.ID));
            }

            // Create lookup dictionaries for O(1) access
            var unitLookup = units.ToDictionary(u => u.ID, u => u.Name);
            var categoryLookup = categories.ToDictionary(c => c.ID, c => c.Name);
            var processingTypeLookup = processingTypes.ToDictionary(p => p.ID, p => p.Name);
            var specialProductTaxRateLookup = specialProductTaxRates.ToDictionary(s => s.ID, s => s.Name);

            // Assign names using lookups
            foreach (var product in products)
            {
                if (product.Unit_ID.HasValue && unitLookup.TryGetValue(product.Unit_ID.Value, out var unitName))
                    product.UnitName = unitName;

                if (product.Category_ID.HasValue && categoryLookup.TryGetValue(product.Category_ID.Value, out var categoryName))
                    product.CategoryName = categoryName;

                if (product.ProcessingType_ID.HasValue && processingTypeLookup.TryGetValue(product.ProcessingType_ID.Value, out var processingTypeName))
                    product.ProcessingTypeName = processingTypeName;

                if (product.SpecialProductTaxRate_ID.HasValue && specialProductTaxRateLookup.TryGetValue(product.SpecialProductTaxRate_ID.Value, out var specialProductTaxRateName))
                    product.SpecialProductTaxRateName = specialProductTaxRateName;
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
        private static Acknowledgement ValidateProductData(ProductViewModel productData)
        {
            var ack = new Acknowledgement { IsSuccess = true };

            if (string.IsNullOrWhiteSpace(productData.Name))
            {
                ack.AddMessage("Tên sản phẩm không được để trống.");
                ack.IsSuccess = false;
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
            existingProduct.ProcessingType_ID = productData.ProcessingType_ID;
            existingProduct.TaxRate = productData.TaxRate;
            existingProduct.LossRate = productData.LossRate;
            existingProduct.IsMaterial = productData.IsMaterial;
            existingProduct.ProfitMargin = productData.ProfitMargin;
            existingProduct.Note = productData.Note;
            existingProduct.IsDiscontinued = productData.IsDiscontinued;
            existingProduct.ProcessingFee = productData.ProcessingFee;
            existingProduct.CompanyTaxRate = productData.CompanyTaxRate;
            existingProduct.ConsumerTaxRate = productData.ConsumerTaxRate;
            existingProduct.SpecialProductTaxRate_ID = productData.SpecialProductTaxRate_ID;
            existingProduct.UpdatedDate = DateTime.Now;
            existingProduct.UpdatedBy = currentUserId;
        }

        #endregion

        #region Public Methods

        public async Task<Acknowledgement<JsonResultPaging<List<ProductViewModel>>>> GetProductList(ProductSearchModel searchModel)
        {
            return await GetProductList(searchModel, null);
        }

        public async Task<Acknowledgement<JsonResultPaging<List<ProductViewModel>>>> GetProductList(ProductSearchModel searchModel, Expression<Func<Product, object>>? selector = null)
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
            var ack = ValidateProductData(postData);
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
            var ack = ValidateProductData(postData);
            if (!ack.IsSuccess) return ack;

            try
            {
                var existingProduct = await _productRepository.Repository.FindAsync(postData.Id);
                if (existingProduct == null)
                {
                    ack.AddMessage("Không tìm thấy sản phẩm.");
                    return ack;
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
                                ProcessingTypeCode = ExcelHelper.GetCellStringValue(row.Cell(5)),
                                IsMaterialText = ExcelHelper.GetCellStringValue(row.Cell(6)),
                                SpecialProductTaxRateCode = ExcelHelper.GetCellStringValue(row.Cell(7)),
                                TaxRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(8))),
                                LossRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(9))),
                                ProfitMargin = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(10))),
                                ProcessingFee = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(11))),
                                DefaultPrice = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(12))),
                                CompanyTaxRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(13))) ?? 0,
                                ConsumerTaxRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(14))) ?? 0,
                                Note = ExcelHelper.GetCellStringValue(row.Cell(15)),
                                IsDiscontinuedText = ExcelHelper.GetCellStringValue(row.Cell(16))
                            };

                            // Validate required fields
                            if (string.IsNullOrEmpty(importModel.Name))
                            {
                                importModel.ValidationErrors.Add("Tên sản phẩm không được để trống");
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
                var processingTypes = await _processingTypeRepository.ReadOnlyRespository.GetAsync(p => p.IsActive);
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

                        int? processingTypeId = null;
                        if (!string.IsNullOrEmpty(importModel.ProcessingTypeCode))
                        {
                            var processingType = processingTypes.FirstOrDefault(p => p.Code.Equals(importModel.ProcessingTypeCode, StringComparison.OrdinalIgnoreCase));
                            if (processingType == null)
                            {
                                importModel.ValidationErrors.Add($"Không tìm thấy loại chế biến với mã: {importModel.ProcessingTypeCode}");
                            }
                            else
                            {
                                processingTypeId = processingType.ID;
                            }
                        }

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
                            ProcessingType_ID = processingTypeId,
                            IsMaterial = importModel.IsMaterial,
                            SpecialProductTaxRate_ID = specialProductTaxRateId,
                            TaxRate = importModel.TaxRate,
                            LossRate = importModel.LossRate,
                            ProfitMargin = importModel.ProfitMargin,
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
                        "Mã danh mục",
                        "Mã loại chế biến",
                        "Là nguyên liệu (Y/N)",
                        "Mã thuế suất đặc biệt",
                        "Thuế suất (%)",
                        "Tỷ lệ hao hụt (%)",
                        "Tỷ lệ lợi nhuận (%)",
                        "Phí chế biến",
                        "Đơn giá mặc định",
                        "Thuế suất công ty (%)",
                        "Thuế suất người tiêu dùng (%)",
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
                    worksheet.Cell(2, 5).Value = "PT001";
                    worksheet.Cell(2, 6).Value = "Y";
                    worksheet.Cell(2, 7).Value = "SPTR001";
                    worksheet.Cell(2, 8).Value = 10;
                    worksheet.Cell(2, 9).Value = 5;
                    worksheet.Cell(2, 10).Value = 15;
                    worksheet.Cell(2, 11).Value = 1000;
                    worksheet.Cell(2, 12).Value = 50000;
                    worksheet.Cell(2, 13).Value = 8;
                    worksheet.Cell(2, 14).Value = 10;
                    worksheet.Cell(2, 15).Value = "Ghi chú mẫu";
                    worksheet.Cell(2, 16).Value = "N";

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
                        "   - Thuế suất công ty (%): Bắt buộc nhập số",
                        "   - Thuế suất người tiêu dùng (%): Bắt buộc nhập số",
                        "",
                        "2. Các cột tùy chọn:",
                        "   - Tên tiếng Anh: Có thể để trống",
                        "   - Mã đơn vị: Phải tồn tại trong hệ thống",
                        "   - Mã danh mục: Phải tồn tại trong hệ thống",
                        "   - Mã loại chế biến: Phải tồn tại trong hệ thống",
                        "   - Là nguyên liệu: Nhập Y/N, Yes/No, True/False, 1/0",
                        "   - Mã thuế suất đặc biệt: Phải tồn tại trong hệ thống",
                        "   - Các tỷ lệ %: Nhập số thập phân (ví dụ: 10.5)",
                        "   - Phí chế biến: Nhập số",
                        "   - Đơn giá mặc định: Nhập số (ví dụ: 50000)",
                        "   - Ghi chú: Có thể để trống",
                        "   - Ngừng sản xuất: Nhập Y/N, Yes/No, True/False, 1/0",
                        "",
                        "3. Lưu ý:",
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
