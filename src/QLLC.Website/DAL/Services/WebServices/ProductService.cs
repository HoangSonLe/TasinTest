using AutoMapper;
using ClosedXML.Excel;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
        private ISpecialProductTaxRateRepository _specialProductTaxRateRepository;

        public ProductService(
            ILogger<ProductService> logger,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IUnitRepository unitRepository,
            ICategoryRepository categoryRepository,
            IProcessingTypeRepository processingTypeRepository,
            IMaterialRepository materialRepository,
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
            _materialRepository = materialRepository;
            _specialProductTaxRateRepository = specialProductTaxRateRepository;
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
                var specialProductTaxRateIds = productViewModels.Where(p => p.SpecialProductTaxRate_ID.HasValue).Select(p => p.SpecialProductTaxRate_ID.Value).Distinct().ToList();

                var units = await _unitRepository.ReadOnlyRespository.GetAsync(i => unitIds.Contains(i.ID));
                var categories = await _categoryRepository.ReadOnlyRespository.GetAsync(i => categoryIds.Contains(i.ID));
                var processingTypes = await _processingTypeRepository.ReadOnlyRespository.GetAsync(i => processingTypeIds.Contains(i.ID));
                var materials = await _materialRepository.ReadOnlyRespository.GetAsync(i => materialIds.Contains(i.ID));
                var specialProductTaxRates = await _specialProductTaxRateRepository.ReadOnlyRespository.GetAsync(i => specialProductTaxRateIds.Contains(i.ID));

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

                    if (product.SpecialProductTaxRate_ID.HasValue)
                    {
                        var specialProductTaxRate = specialProductTaxRates.FirstOrDefault(s => s.ID == product.SpecialProductTaxRate_ID.Value);
                        product.SpecialProductTaxRateName = specialProductTaxRate?.Name;
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

                if (product.SpecialProductTaxRate_ID.HasValue)
                {
                    var specialProductTaxRate = await _specialProductTaxRateRepository.ReadOnlyRespository.FindAsync(product.SpecialProductTaxRate_ID.Value);
                    productViewModel.SpecialProductTaxRateName = specialProductTaxRate?.Name;
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
                    existingProduct.LossRate = postData.LossRate;
                    existingProduct.Material_ID = postData.Material_ID;
                    existingProduct.ProfitMargin = postData.ProfitMargin;
                    existingProduct.Note = postData.Note;
                    existingProduct.IsDiscontinued = postData.IsDiscontinued;
                    existingProduct.ProcessingFee = postData.ProcessingFee;
                    existingProduct.CompanyTaxRate = postData.CompanyTaxRate;
                    existingProduct.ConsumerTaxRate = postData.ConsumerTaxRate;
                    existingProduct.SpecialProductTaxRate_ID = postData.SpecialProductTaxRate_ID;
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
                                MaterialCode = ExcelHelper.GetCellStringValue(row.Cell(6)),
                                SpecialProductTaxRateCode = ExcelHelper.GetCellStringValue(row.Cell(7)),
                                TaxRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(8))),
                                LossRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(9))),
                                ProfitMargin = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(10))),
                                ProcessingFee = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(11))),
                                CompanyTaxRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(12))) ?? 0,
                                ConsumerTaxRate = ParseDecimal(ExcelHelper.GetCellStringValue(row.Cell(13))) ?? 0,
                                Note = ExcelHelper.GetCellStringValue(row.Cell(14)),
                                IsDiscontinuedText = ExcelHelper.GetCellStringValue(row.Cell(15))
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
                var materials = await _materialRepository.ReadOnlyRespository.GetAsync(m => m.IsActive);
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

                        int? materialId = null;
                        if (!string.IsNullOrEmpty(importModel.MaterialCode))
                        {
                            var material = materials.FirstOrDefault(m => m.Code.Equals(importModel.MaterialCode, StringComparison.OrdinalIgnoreCase));
                            if (material == null)
                            {
                                importModel.ValidationErrors.Add($"Không tìm thấy nguyên liệu với mã: {importModel.MaterialCode}");
                            }
                            else
                            {
                                materialId = material.ID;
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
                            Material_ID = materialId,
                            SpecialProductTaxRate_ID = specialProductTaxRateId,
                            TaxRate = importModel.TaxRate,
                            LossRate = importModel.LossRate,
                            ProfitMargin = importModel.ProfitMargin,
                            ProcessingFee = importModel.ProcessingFee,
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

        public async Task<byte[]> GenerateProductExcelTemplate()
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
                        "Mã nguyên liệu",
                        "Mã thuế suất đặc biệt",
                        "Thuế suất (%)",
                        "Tỷ lệ hao hụt (%)",
                        "Tỷ lệ lợi nhuận (%)",
                        "Phí chế biến",
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
                    worksheet.Cell(2, 6).Value = "MAT001";
                    worksheet.Cell(2, 7).Value = "SPTR001";
                    worksheet.Cell(2, 8).Value = 10;
                    worksheet.Cell(2, 9).Value = 5;
                    worksheet.Cell(2, 10).Value = 15;
                    worksheet.Cell(2, 11).Value = 1000;
                    worksheet.Cell(2, 12).Value = 8;
                    worksheet.Cell(2, 13).Value = 10;
                    worksheet.Cell(2, 14).Value = "Ghi chú mẫu";
                    worksheet.Cell(2, 15).Value = "N";

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
                        "   - Mã nguyên liệu: Phải tồn tại trong hệ thống",
                        "   - Mã thuế suất đặc biệt: Phải tồn tại trong hệ thống",
                        "   - Các tỷ lệ %: Nhập số thập phân (ví dụ: 10.5)",
                        "   - Phí chế biến: Nhập số",
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

                    return ExcelHelper.SaveToByteArray(workbook);
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
    }
}
