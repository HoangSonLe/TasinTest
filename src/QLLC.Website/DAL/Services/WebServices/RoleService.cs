using LinqKit;
using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Services;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.DAL.Services.WebServices
{
    public class RoleService : BaseService<RoleService>, IRoleService
    {
        public RoleService(
            ILogger<RoleService> logger,
            IConfiguration configuration,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserContext currentUserContext
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext)
        {
        }

        public async Task<Acknowledgement<List<KendoDropdownListModel<int>>>> GetRoleDropdownList(string searchString)
        {
            var response = new Acknowledgement<List<KendoDropdownListModel<int>>>();
            try
            {
                var maxLevel = Utils.GetMaxLevelRole(_currentUserRoleId);
                var predicate = PredicateBuilder.New<Role>(i=> i.Level > maxLevel);
                if(!string.IsNullOrEmpty(searchString))
                {
                    searchString = Utils.NonUnicode(searchString.Trim().ToLower());
                    predicate = predicate.And(i=> i.NameNonUnicode.Trim().ToLower() == searchString.ToLower());
                }
                var roleList = (await _roleRepository.Repository.GetAsync(predicate))
                                                   .Select(i => new KendoDropdownListModel<int>()
                                                   {
                                                       Value = i.Id.ToString(),
                                                       Text = i.Description,
                                                   })
                                                   .ToList();
                response.Data = roleList;
                response.IsSuccess = true;
                _logger.LogError("GetUserList " + "Tét");
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
