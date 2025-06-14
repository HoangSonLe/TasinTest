using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Models.ViewModels;
using System.Diagnostics;
using Tasin.Website.Common.Services;

namespace Tasin.Website.Controllers
{
    [Authorize]
    public class HomeController : BaseController<HomeController>
    {
        private readonly IConfiguration _configuration;
        public HomeController(
            ILogger<HomeController> logger,
            IUserService userService,
            IRoleService roleService,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext
            ) : base(logger, userService, currentUserContext)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult NotificationDemo()
        {
            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult TestStaticFiles()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "test-static-files.html");
            if (System.IO.File.Exists(filePath))
            {
                var content = System.IO.File.ReadAllText(filePath);
                return Content(content, "text/html");
            }
            return NotFound("Test file not found");
        }


        [AllowAnonymous]
        public string Values()
        {
            return "Server is running";
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        public IActionResult QRCode()
        {
            var ack = new Acknowledgement<string>();
            try
            {
                var urlBotTelegram = _configuration.GetSection("TelegramBotUrl").Value;
                var qrCode = Helper.GenerateQrCodeAsBase64(urlBotTelegram);
                ack.IsSuccess = true;
                ack.Data = qrCode;
                return Json(ack);
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                return Json(ack);
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Path to the HTML file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/403Page.html");

            // Read the HTML file content
            var htmlContent = System.IO.File.ReadAllText(filePath);

            // Pass the HTML content to the view
            ViewBag.HtmlContent = htmlContent;

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
