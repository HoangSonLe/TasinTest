using Microsoft.AspNetCore.Mvc;
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
    [Route("api/[controller]/GetCustomerList")]
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

        //[C3FunctionAuthorization(true, functionIdList: [(int)EActionRole.READ_USER])]
        [HttpGet]
        [Route("Customer/Index")]
        public IActionResult Index()
        {
            ViewBag.CustomerTypeDatasource = EnumHelper.ToDropdownList<ECustomerType>();
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
        public async Task<IActionResult> GetCustomerList([FromQuery]CustomerSearchModel searchModel)
        {
            var result = await _customerService.GetCustomerList(searchModel);
            return Json(result);
        }
        [HttpDelete]
        [Route("Customer/DeleteCustomerById")]
        public async Task<Acknowledgement> DeleteCustomerById(int customerId)
        {
            return await _customerService.DeleteCustomerById(customerId);
        }
       
        [HttpPost]
        [Route("Customer/Create")]
        public async Task<Acknowledgement> Create([FromBody] CustomerViewModel postData)
        {
            return await _customerService.CreateOrUpdateCustomer(postData);
        }

        [HttpPut]
        [Route("Customer/UpdateCustomer/{customerId}")]
        public async Task<Acknowledgement> CreateOrUpdateCustomer([FromRoute]int customerId, [FromBody] CustomerViewModel postData)
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
        [Route("api/[controller]/GetCustomerById/{userId}")]
        public async Task<Acknowledgement<CustomerViewModel>> GetCustomerById(int userId)
        {
            var ack = await _customerService.GetCustomerById(userId);
            return ack;
        }
    }
}
