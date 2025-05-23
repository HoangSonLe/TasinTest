using LinqKit;
using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Services.AuthorPredicates;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Services.WebServices
{
    public class CommonService : BaseService<CommonService>, ICommonService
    {
        public CommonService(
            ILogger<CommonService> logger,
            IConfiguration configuration,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
        }


        private List<KendoDropdownListModel<string>> GetCustomerType(string? searchString)
        {
            var options = EnumHelper.ToDropdownList<ECustomerType>();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                options = options.SearchWithoutDiacriticsInMemory(i => i.Text, searchString).ToList();
            }
            return options;
        }
        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetDataOptionsDropdown(string? searchString, ECategoryType type)
        {
            var response = new Acknowledgement<List<KendoDropdownListModel<string>>>()
            {
                Data = new List<KendoDropdownListModel<string>>(),
                IsSuccess = true
            };
            try
            {
                switch (type)
                {
                    case ECategoryType.CustomerType:
                        response.Data = GetCustomerType(searchString);
                        break;

                    default: break;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError("GetUserList " + ex.Message);
                return response;

            }
        }
    }
}
