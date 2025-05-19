using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IUserService : IBaseService, IDisposable
    {
        Task<Acknowledgement<UserViewModel>> Login(LoginViewModel loginModel);
        Task<Acknowledgement> LockUser(string userName);

        Task<Acknowledgement<JsonResultPaging<List<UserViewModel>>>> GetUserList(UserSearchModel postData);
        Task<Acknowledgement<UserViewModel>> GetUserById(int userId);
        Task<Acknowledgement> CreateOrUpdateUser(UserViewModel postData);

        Task<Acknowledgement> DeleteUserById(int userId);
        Task<Acknowledgement> ResetUserPasswordById(int userId);
        Task<Acknowledgement> ChangePassword([FromBody] ChangePasswordModel postData);

        Task<Acknowledgement<List<KendoDropdownListModel<int>>>> GetUserDataDropdownList(string searchString, List<int> selectedIdList);

    }
}
