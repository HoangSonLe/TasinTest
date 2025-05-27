using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.SecurityLogin;
using Tasin.Website.Common.ConfigModel;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Services;
using Tasin.Website.DAL.Services.Interfaces;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.ViewModels.AccountViewModels;
using System.Security.Claims;
using Tasin.Website.Common.Services;

namespace Tasin.Website.Controllers
{
    [Authorize]
    public class TestController : BaseController<TestController>
    {
        private readonly IUserService _userService;
        private readonly IInvoiceService _invoiceService;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailSender;
        private readonly IHttpContextAccessor _accessor;

        public object ContextType { get; private set; }

        public TestController(
            IUserService userService,
            IInvoiceService invoiceService,
            ILogger<TestController> logger,
            IConfiguration configuration,
            EmailService emailService,
            ICurrentUserContext currentUserContext
            ) : base(logger, userService, currentUserContext)
        {
            _userService = userService;
            _invoiceService = invoiceService;
            _configuration = configuration;
            _emailSender = emailService;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task Test([FromHeader] string mail)
        {
            await _emailSender.SendEmailAsync(mail);
        }

        /// <summary>
        /// Test invoice generation functionality
        /// </summary>
        /// <param name="purchaseOrderId">Purchase order ID to test with</param>
        /// <returns>Test result</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("Test/TestInvoice")]
        public async Task<IActionResult> TestInvoice(int purchaseOrderId = 1)
        {
            try
            {
                // Test invoice generation
                var invoiceResult = await _invoiceService.GenerateInvoiceFromPurchaseOrder(purchaseOrderId);

                if (!invoiceResult.IsSuccess)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to generate invoice",
                        errors = invoiceResult.ErrorMessageList
                    });
                }

                // Test HTML generation
                var htmlResult = await _invoiceService.GenerateInvoiceHtml(invoiceResult.Data);

                return Json(new
                {
                    success = true,
                    message = "Invoice functionality test completed successfully",
                    invoiceCode = invoiceResult.Data.InvoiceCode,
                    totalAmount = invoiceResult.Data.TotalAmount,
                    itemCount = invoiceResult.Data.Items.Count,
                    htmlLength = htmlResult.Length
                });
            }
            catch (Exception ex)
            {
                Logger.LogError($"TestInvoice: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
