using System;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Services;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.Domains.DBContexts;
using System.Security.Claims;

namespace Tasin.Website.DAL.Services.WebServices
{
    public abstract class BaseService<T> : IDisposable
    {
        public readonly ILogger<T> _logger;
        public readonly IUserRepository _userRepository;
        public readonly IRoleRepository _roleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IConfiguration _configuration;
        public IConfiguration Configuration => _configuration;

        // Properties to access user context information
        public int CurrentUserId => _currentUserContext.UserId ?? throw new InvalidOperationException("User is not authenticated");
        public List<ERoleType> CurrentUserRoles => _currentUserContext.UserRoles;
        public int? CurrentTenantId => _currentUserContext.TenantId;
        public bool IsAuthenticated => _currentUserContext.IsAuthenticated;

        // For backward compatibility - will be deprecated
        [Obsolete("Use CurrentUserId property instead")]
        public int _currentUserId => CurrentUserId;

        [Obsolete("Use CurrentUserRoles property instead")]
        public List<ERoleType> _currentUserRoleId => CurrentUserRoles;

        [Obsolete("Use CurrentTenantId property instead")]
        public int? _currentTenantId => CurrentTenantId;

        public BaseService(
            ILogger<T> logger,
            IConfiguration configuration,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserContext currentUserContext)
        {
            _logger = logger;
            _configuration = configuration;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
        }
        private SampleDBContext _DbContext;
        public SampleDBContext DbContext
        {
            get
            {

                if (_DbContext == null)
                {
                    _DbContext = new SampleDBContext();
                }
                return _DbContext;
            }
        }
        public void Dispose()
        {
            DbContext.Dispose();
        }
        ~BaseService()
        {
            Dispose();
        }
        #region COMMON FUNC AUTHOR
        /// <summary>
        /// Tạm - Làm chức năng phân quyền sau
        /// </summary>
        /// <returns></returns>
        public bool _IsHasAdminRole()
        {
            return _currentUserContext.IsAdmin;
        }
        #endregion

    }
}
