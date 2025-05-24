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

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailSender;
        private readonly IHttpContextAccessor _accessor;

        public object ContextType { get; private set; }

        public TestController(
            IUserService userService,
            ILogger<TestController> logger,
            IConfiguration configuration,
            EmailService emailService,
            ICurrentUserContext currentUserContext
            ) : base(logger, userService, currentUserContext)
        {
            _userService = userService;
            _configuration = configuration;
            _emailSender = emailService;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task Test([FromHeader]string mail)
        {
            await _emailSender.SendEmailAsync(mail);
        }
    }
}
