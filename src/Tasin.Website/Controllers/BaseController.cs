using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Tasin.Website.Common.ConfigModel;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Services.WebInterfaces;

namespace Tasin.Website.Controllers
{
    public class BaseController<T> : Controller
    {
        private readonly ILogger<T> _logger;
        private readonly IUserService _userService;
        private readonly ICurrentUserContext _currentUserContext;

        protected readonly SiteUIConfigs UIConfigs;

        // For backward compatibility - will be deprecated
        [Obsolete("Use CurrentUserContext.UserId property instead")]
        public string _currentUserId => _currentUserContext.UserId?.ToString();

        public bool _isMobile => bool.Parse(HttpContext.User.FindFirstValue("IsMobile") ?? "false");

        public BaseController(ILogger<T> logger, IUserService userService, ICurrentUserContext currentUserContext)
        {
            _logger = logger;
            _userService = userService;
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
        }

        public IUserService UserService => _userService;
        public ILogger<T> Logger => _logger;
        public ICurrentUserContext CurrentUserContext => _currentUserContext;
    }
}
