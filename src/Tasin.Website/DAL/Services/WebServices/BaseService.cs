﻿using System;
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
        private readonly SampleDBContext _dbContext;
        public IConfiguration Configuration => _configuration;
        public SampleDBContext DbContext => _dbContext;

        // Properties to access user context information
        public int CurrentUserId => _currentUserContext.UserId ?? throw new InvalidOperationException("User is not authenticated");
        public List<ERoleType> CurrentUserRoles => _currentUserContext.UserRoles;
        public bool IsAuthenticated => _currentUserContext.IsAuthenticated;

        public BaseService(
            ILogger<T> logger,
            IConfiguration configuration,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public void Dispose()
        {
            DbContext.Dispose();
            GC.SuppressFinalize(this);
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
