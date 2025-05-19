using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    public class UserSearchModel : SearchPagingModel<UserViewModel>
    {
        public string SearchString { get; set; }
        public string RoleIdListString { get; set; }
        public List<int> RoleIdList => !string.IsNullOrEmpty(RoleIdListString) ? RoleIdListString.Split(",").Select(i => Int32.Parse(i)).ToList() : new List<int>();
    }
}
