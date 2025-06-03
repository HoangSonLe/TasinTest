using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Authorizations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Controllers
{
    /// <summary>
    /// Controller for managing users
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class CustomerController : BaseController<CustomerController>
    {
        private ICustomerService _customerService;
        public CustomerController(
            ICustomerService customerService,
            IUserService userService,
            ILogger<CustomerController> logger,
            ICurrentUserContext currentCustomerContext) : base(logger, userService, currentCustomerContext)
        {
            _customerService = customerService;
        }

        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_CUSTOMER])]
        [HttpGet]
        [Route("Customer/Index")]
        public IActionResult Index()
        {
            ViewBag.CustomerTypeDatasource = EnumHelper.ToDropdownListStr<ECustomerType>();
            return View();
        }


        /// <summary>
        /// Get a list of users with pagination and filtering
        /// </summary>
        /// <param name="searchModel">Search parameters</param>
        /// <returns>List of users</returns>
        /// <response code="200">Returns the list of users</response>
        [HttpGet]
        [Route("Customer/GetCustomerList")]
        [ProducesResponseType(typeof(Acknowledgement<JsonResultPaging<List<CustomerViewModel>>>), 200)]
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_CUSTOMER])]
        public async Task<IActionResult> GetCustomerList([FromQuery] CustomerSearchModel searchModel)
        {
            var result = await _customerService.GetCustomerList(searchModel);
            return Json(result);
        }
        [HttpDelete]
        [Route("Customer/DeleteCustomerById/{customerId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.DELETE_CUSTOMER])]
        public async Task<Acknowledgement> DeleteCustomerById([FromRoute] int customerId)
        {
            return await _customerService.DeleteCustomerById(customerId);
        }

        [HttpPost]
        [Route("Customer/Create")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_CUSTOMER])]
        public async Task<Acknowledgement> Create([FromBody] CustomerViewModel postData)
        {
            return await _customerService.CreateOrUpdateCustomer(postData);
        }

        [HttpPut]
        [Route("Customer/UpdateCustomer/{customerId}")]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.UPDATE_CUSTOMER])]
        public async Task<Acknowledgement> CreateOrUpdateCustomer([FromRoute] int customerId, [FromBody] CustomerViewModel postData)
        {
            postData.Id = customerId;
            return await _customerService.CreateOrUpdateCustomer(postData);
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="userId">Customer ID</param>
        /// <returns>Customer details</returns>
        /// <response code="200">Returns the user details</response>
        /// <response code="404">If the user is not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(Acknowledgement<CustomerViewModel>), 200)]
        [ProducesResponseType(404)]
        [Route("Customer/GetCustomerById/{userId}")]
        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_CUSTOMER])]
        public async Task<Acknowledgement<CustomerViewModel>> GetCustomerById(int userId)
        {
            var ack = await _customerService.GetCustomerById(userId);
            return ack;
        }

        /// <summary>
        /// Import customers from Excel file
        /// </summary>
        /// <param name="file">Excel file to import</param>
        /// <returns>Import result</returns>
        [HttpPost]
        [Route("Customer/ImportExcel")]
        [ProducesResponseType(typeof(Acknowledgement<CustomerExcelImportResult>), 200)]
        [C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.CREATE_CUSTOMER])]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            try
            {
                var result = await _customerService.ImportCustomersFromExcel(file);
                return Json(result);
            }
            catch (Exception ex)
            {
                Logger.LogError($"ImportExcel: {ex.Message}");
                return Json(new Acknowledgement<CustomerExcelImportResult>
                {
                    IsSuccess = false,
                    ErrorMessageList = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Download Excel template for customer import
        /// </summary>
        /// <returns>Excel template file</returns>
        [HttpGet]
        [Route("Customer/DownloadTemplate")]
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                var templateBytes = await _customerService.GenerateCustomerExcelTemplate();
                var fileName = $"Customer_Import_Template_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                // Set proper content type and encoding for Excel files with UTF-8 support
                Response.Headers.Add("Content-Disposition", ExcelHelper.GetContentDispositionHeader(fileName));

                return File(templateBytes, ExcelHelper.GetExcelContentType(), fileName);
            }
            catch (Exception ex)
            {
                Logger.LogError($"DownloadTemplate: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
