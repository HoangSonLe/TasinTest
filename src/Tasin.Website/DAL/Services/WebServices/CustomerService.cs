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
using Tasin.Website.Models.ViewModels.AccountViewModels;
using ClosedXML.Excel;


namespace Tasin.Website.DAL.Services.WebServices
{
    public class CustomerService : BaseService<CustomerService>, ICustomerService
    {
        private readonly IMapper _mapper;
        private ICustomerRepository _customerRepository;
        public CustomerService(
            ILogger<CustomerService> logger,
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _customerRepository = customerRepository;
        }


        public async Task<Acknowledgement<JsonResultPaging<List<CustomerViewModel>>>> GetCustomerList(CustomerSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<CustomerViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Customer>(i => i.IsActive == true);

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => i.NameNonUnicode.ToLower().Contains(searchStringNonUnicode) ||
                                                    (string.IsNullOrEmpty(i.PhoneContact) == false && i.PhoneContact.Trim().ToLower().Contains(searchStringNonUnicode))
                                             );
                }

                var userList = new List<CustomerViewModel>();
                predicate = CustomerAuthorPredicate.GetCustomerAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);
                var customerQuery = await _customerRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(searchModel.PageNumber, searchModel.PageSize),
                    predicate,
                    i => i.OrderByDescending(u => u.UpdatedDate)
                    );
                var customerDBList = _mapper.Map<List<CustomerViewModel>>(customerQuery.Data);
                var updateByUserIdList = customerDBList.Select(i => i.UpdatedBy).ToList();
                var updateByUserList = await _userRepository.ReadOnlyRespository.GetAsync(i => updateByUserIdList.Contains(i.Id));
                foreach (var user in customerDBList)
                {
                    var updateUser = updateByUserList.FirstOrDefault(i => i.Id == user.UpdatedBy);
                    user.UpdatedByName = updateUser.Name;
                }
                response.Data = new JsonResultPaging<List<CustomerViewModel>>()
                {
                    Data = customerDBList,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = customerQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError("GetCustomerList " + ex.Message);
                return response;

            }
        }

        public async Task<Acknowledgement<CustomerViewModel>> GetCustomerById(int userId)
        {
            var ack = new Acknowledgement<CustomerViewModel>();
            try
            {
                var customer = await _customerRepository.ReadOnlyRespository.FindAsync(userId);
                if (customer == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy khách hàng");
                    return ack;
                }

                ack.Data = _mapper.Map<CustomerViewModel>(customer);
                ack.IsSuccess = true;
                return ack;

            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError("GetCustomerById " + ex.Message);
                return ack;
            }

        }

        public async Task<Acknowledgement> CreateOrUpdateCustomer(CustomerViewModel postData)
        {
            var ack = new Acknowledgement();
            if (string.IsNullOrWhiteSpace(postData.Name))
            {
                ack.AddMessage("Vui lòng nhập họ tên");
                return ack;
            }
            if (postData.Type == ECustomerType.Company && string.IsNullOrWhiteSpace(postData.TaxCode))
            {
                ack.AddMessage("Vui lòng nhập mã số thuế");
                return ack;
            }
            var phone = postData.PhoneContact;
            if (!string.IsNullOrEmpty(phone))
            {
                var validatePhoneMessage = Validate.ValidPhoneNumber(ref phone);
                if (validatePhoneMessage != null)
                {
                    ack.AddMessage(validatePhoneMessage);
                    return ack;
                }
                postData.PhoneContact = phone;

            }
            if (!string.IsNullOrWhiteSpace(postData.Email))
            {
                var isValidEmail = Validate.ValidEmail(postData.Email);
                if (!isValidEmail)
                {
                    ack.AddMessage("Email không đúng định dạng.");
                    return ack;
                }
            }

            if (postData.Id == 0)
            {
                var newCustomer = _mapper.Map<Customer>(postData);
                newCustomer.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Customer, DbContext);
                newCustomer.NameNonUnicode = Utils.NonUnicode(newCustomer.Name);
                newCustomer.CreatedDate = DateTime.Now;
                newCustomer.CreatedBy = CurrentUserId;
                newCustomer.UpdatedDate = newCustomer.CreatedDate;
                newCustomer.UpdatedBy = newCustomer.CreatedBy;
                await ack.TrySaveChangesAsync(res => res.AddAsync(newCustomer), _customerRepository.Repository);
            }
            else
            {
                var existItem = await _customerRepository.Repository.FirstOrDefaultAsync(i => i.ID == postData.Id && i.IsActive == true);
                if (existItem == null)
                {
                    ack.AddMessage("Không tìm thấy khách hàng");
                    ack.IsSuccess = false;
                    return ack;
                }
                else
                {
                    existItem.Name = postData.Name;
                    existItem.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existItem.PhoneContact = postData.PhoneContact;
                    existItem.Email = postData.Email;
                    existItem.TaxCode = postData.TaxCode;
                    existItem.Address = postData.Address;
                    existItem.UpdatedDate = DateTime.Now;
                    existItem.UpdatedBy = CurrentUserId;
                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existItem), _customerRepository.Repository);
                }
            }
            return ack;
        }

        public async Task<Acknowledgement> DeleteCustomerById(int userId)
        {
            var ack = new Acknowledgement();
            var customer = await _customerRepository.Repository.FirstOrDefaultAsync(i => i.ID == userId);
            if (customer == null)
            {
                ack.AddMessage("Không tìm thấy khách hàng");
                return ack;
            }
            customer.IsActive = false;
            await ack.TrySaveChangesAsync(res => res.UpdateAsync(customer), _customerRepository.Repository);
            return ack;
        }

        public async Task<Acknowledgement<CustomerExcelImportResult>> ImportCustomersFromExcel(IFormFile file)
        {
            var response = new Acknowledgement<CustomerExcelImportResult>();
            var result = new CustomerExcelImportResult();

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
                var importModels = new List<CustomerExcelImportModel>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = ExcelHelper.CreateWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1); // Skip header row

                        int rowNumber = 2; // Start from row 2 (after header)
                        foreach (var row in rows)
                        {
                            var importModel = new CustomerExcelImportModel
                            {
                                RowNumber = rowNumber,
                                Name = ExcelHelper.GetCellStringValue(row.Cell(1)),
                                TypeText = ExcelHelper.GetCellStringValue(row.Cell(2)),
                                PhoneContact = ExcelHelper.GetCellStringValue(row.Cell(3)),
                                Email = ExcelHelper.GetCellStringValue(row.Cell(4)),
                                TaxCode = ExcelHelper.GetCellStringValue(row.Cell(5)),
                                Address = ExcelHelper.GetCellStringValue(row.Cell(6))
                            };

                            // Skip empty rows
                            if (string.IsNullOrWhiteSpace(importModel.Name))
                            {
                                rowNumber++;
                                continue;
                            }

                            importModels.Add(importModel);
                            rowNumber++;
                        }
                    }
                }

                result.TotalRows = importModels.Count;

                // Process each row
                foreach (var importModel in importModels)
                {
                    try
                    {
                        // Validate required fields
                        var errors = new List<string>();

                        if (string.IsNullOrWhiteSpace(importModel.Name))
                        {
                            errors.Add("Tên khách hàng không được để trống");
                        }

                        if (string.IsNullOrWhiteSpace(importModel.TypeText))
                        {
                            errors.Add("Loại khách hàng không được để trống");
                        }

                        // Parse customer type
                        ECustomerType customerType = ECustomerType.Individual;
                        if (!string.IsNullOrWhiteSpace(importModel.TypeText))
                        {
                            var typeText = importModel.TypeText.Trim().ToLower();
                            if (typeText == "doanh nghiệp" || typeText == "company" || typeText == "0")
                            {
                                customerType = ECustomerType.Company;
                            }
                            else if (typeText == "cá nhân" || typeText == "individual" || typeText == "1")
                            {
                                customerType = ECustomerType.Individual;
                            }
                            else
                            {
                                errors.Add($"Loại khách hàng không hợp lệ: {importModel.TypeText}. Chỉ chấp nhận: 'Doanh nghiệp', 'Cá nhân', 'Company', 'Individual', '0', '1'");
                            }
                        }

                        // Validate tax code for company
                        if (customerType == ECustomerType.Company && string.IsNullOrWhiteSpace(importModel.TaxCode))
                        {
                            errors.Add("Mã số thuế không được để trống đối với doanh nghiệp");
                        }

                        // Validate phone number
                        var phone = importModel.PhoneContact;
                        if (!string.IsNullOrEmpty(phone))
                        {
                            var validatePhoneMessage = Validate.ValidPhoneNumber(ref phone);
                            if (validatePhoneMessage != null)
                            {
                                errors.Add(validatePhoneMessage);
                            }
                        }

                        // Validate email
                        if (!string.IsNullOrWhiteSpace(importModel.Email))
                        {
                            var isValidEmail = Validate.ValidEmail(importModel.Email);
                            if (!isValidEmail)
                            {
                                errors.Add("Email không đúng định dạng");
                            }
                        }

                        if (errors.Any())
                        {
                            result.FailedRows++;
                            result.Errors.Add(new CustomerExcelImportError
                            {
                                RowNumber = importModel.RowNumber,
                                CustomerName = importModel.Name,
                                ErrorMessages = errors
                            });
                            continue;
                        }

                        // Create customer
                        var customer = new Customer
                        {
                            Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Customer, DbContext),
                            Name = importModel.Name.Trim(),
                            NameNonUnicode = Utils.NonUnicode(importModel.Name.Trim()),
                            Type = customerType.ToString(),
                            PhoneContact = string.IsNullOrEmpty(phone) ? null : phone,
                            Email = string.IsNullOrEmpty(importModel.Email) ? null : importModel.Email.Trim(),
                            TaxCode = string.IsNullOrEmpty(importModel.TaxCode) ? null : importModel.TaxCode.Trim(),
                            Address = string.IsNullOrEmpty(importModel.Address) ? null : importModel.Address.Trim(),
                            CreatedDate = DateTime.Now,
                            CreatedBy = CurrentUserId,
                            UpdatedDate = DateTime.Now,
                            UpdatedBy = CurrentUserId,
                            IsActive = true,
                            Status = ECommonStatus.Actived.ToString()
                        };

                        await _customerRepository.Repository.AddAsync(customer);
                        result.SuccessfulRows++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedRows++;
                        result.Errors.Add(new CustomerExcelImportError
                        {
                            RowNumber = importModel.RowNumber,
                            CustomerName = importModel.Name,
                            ErrorMessages = new List<string> { $"Lỗi xử lý: {ex.Message}" }
                        });
                        _logger.LogError($"Error processing row {importModel.RowNumber}: {ex.Message}");
                    }
                }

                // Save changes
                await DbContext.SaveChangesAsync();

                response.Data = result;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"ImportCustomersFromExcel: {ex.Message}");
                return response;
            }
        }

        public Task<byte[]> GenerateCustomerExcelTemplate()
        {
            try
            {
                using (var workbook = ExcelHelper.CreateWorkbook())
                {
                    // Create main data sheet
                    var dataSheet = workbook.Worksheets.Add("Khách hàng");

                    // Set headers
                    var headers = new[]
                    {
                        "Tên khách hàng (*)",
                        "Loại khách hàng (*)",
                        "Số điện thoại",
                        "Email",
                        "Mã số thuế",
                        "Địa chỉ"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = dataSheet.Cell(1, i + 1);
                        ExcelHelper.SetCellValue(cell, headers[i]);
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    // Add sample data
                    var sampleData = new[]
                    {
                        new[] { "Công ty TNHH ABC", "Doanh nghiệp", "0123456789", "abc@company.com", "0123456789", "123 Đường ABC, Quận 1, TP.HCM" },
                        new[] { "Nguyễn Văn A", "Cá nhân", "0987654321", "nguyenvana@email.com", "", "456 Đường XYZ, Quận 2, TP.HCM" }
                    };

                    for (int row = 0; row < sampleData.Length; row++)
                    {
                        for (int col = 0; col < sampleData[row].Length; col++)
                        {
                            ExcelHelper.SetCellValue(dataSheet.Cell(row + 2, col + 1), sampleData[row][col]);
                        }
                    }

                    // Auto-fit columns
                    dataSheet.Columns().AdjustToContents();

                    // Create instruction sheet
                    var instructionSheet = workbook.Worksheets.Add("Hướng dẫn");
                    ExcelHelper.SetCellValue(instructionSheet.Cell(1, 1), "HƯỚNG DẪN IMPORT KHÁCH HÀNG");
                    instructionSheet.Cell(1, 1).Style.Font.Bold = true;
                    instructionSheet.Cell(1, 1).Style.Font.FontSize = 16;

                    var instructions = new[]
                    {
                        "",
                        "1. Cấu trúc file:",
                        "   - Tên khách hàng (*): Bắt buộc, tên đầy đủ của khách hàng",
                        "   - Loại khách hàng (*): Bắt buộc, nhập 'Doanh nghiệp' hoặc 'Cá nhân'",
                        "   - Số điện thoại: Tùy chọn, định dạng số điện thoại hợp lệ",
                        "   - Email: Tùy chọn, định dạng email hợp lệ",
                        "   - Mã số thuế: Bắt buộc đối với doanh nghiệp",
                        "   - Địa chỉ: Tùy chọn, địa chỉ liên hệ",
                        "",
                        "2. Quy tắc nhập liệu:",
                        "   - Các trường có dấu (*) là bắt buộc",
                        "   - Loại khách hàng: Nhập 'Doanh nghiệp', 'Cá nhân', 'Company', 'Individual', '0' (Doanh nghiệp), '1' (Cá nhân)",
                        "   - Mã số thuế: Bắt buộc đối với loại 'Doanh nghiệp'",
                        "   - Email: Phải đúng định dạng email (có chứa @ và domain)",
                        "   - Số điện thoại: Phải là số điện thoại hợp lệ",
                        "",
                        "3. Lưu ý:",
                        "   - Không được xóa dòng tiêu đề",
                        "   - Dữ liệu bắt đầu từ dòng 2",
                        "   - File phải có định dạng Excel (.xlsx, .xls, .xlsm) hoặc CSV",
                        "   - Hệ thống sẽ tự động tạo mã khách hàng"
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
                _logger.LogError($"GenerateCustomerExcelTemplate: {ex.Message}");
                throw;
            }
        }
    }
}
