using Tasin.Website.Common.Enums;
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
        private readonly IConfiguration _configuration;
        public IConfiguration Configuration => _configuration;
        public int _currentUserId;
        public List<ERoleType> _currentUserRoleId;
        public int? _currentTenantId;

        public BaseService(
            ILogger<T> logger,
            IConfiguration configuration,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserID");
            if(_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && userId == null)
            {
                throw new ArgumentException("Claim must have UserID.");
            }
            if(_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && !Int32.TryParse(userId, out _currentUserId))
            {
                throw new ArgumentException("Claim UserID is correct format.");
            }
            var roleListClaim = _httpContextAccessor.HttpContext.User.FindFirstValue("RoleIds");
            if(!string.IsNullOrEmpty(roleListClaim))
            {
                _currentUserRoleId = roleListClaim.Split(",").Select(i=>(ERoleType)Int32.Parse(i)).ToList();
            }
            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
            {
                var tenantId = _httpContextAccessor.HttpContext.User.FindFirstValue("TenantId");
                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    _currentTenantId = Int32.Parse(tenantId);
                }
            }
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
            if(_currentUserRoleId == null)
            {
                throw new NullReferenceException("Không tìm thấy roleId in context");
            }
            if(_currentUserRoleId.Contains(ERoleType.Admin) || _currentUserRoleId.Contains(ERoleType.SystemAdmin))
            {
                return true;
            }
            return false;
        }
        #endregion

    }
}
