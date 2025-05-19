using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.ConfigModel;
using Tasin.Website.DAL.Services.WebInterfaces;
using System.Security.Claims;

namespace Tasin.Website.Controllers
{
    public class BaseController<T> : Controller
    {
        private readonly ILogger<T> _logger;
        private readonly IUserService _userService;

        protected readonly SiteUIConfigs UIConfigs;
        public string _currentUserId => HttpContext.User.FindFirstValue("UserID");
        public bool _isMobile => bool.Parse(HttpContext.User.FindFirstValue("IsMobile"));


        public BaseController(ILogger<T> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        public IUserService UserService => _userService;
        public ILogger<T> Logger => _logger;
    }
}
