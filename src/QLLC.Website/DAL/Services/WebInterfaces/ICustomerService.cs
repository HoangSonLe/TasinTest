using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface ICustomerService : IBaseService, IDisposable
    {

        Task<Acknowledgement<JsonResultPaging<List<CustomerViewModel>>>> GetCustomerList(CustomerSearchModel postData);
        Task<Acknowledgement<CustomerViewModel>> GetCustomerById(int userId);
        Task<Acknowledgement> CreateOrUpdateCustomer(CustomerViewModel postData);
        Task<Acknowledgement> DeleteCustomerById(int userId);
    }
}
