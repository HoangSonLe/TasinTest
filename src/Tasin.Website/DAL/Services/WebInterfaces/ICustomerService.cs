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

        /// <summary>
        /// Import customers from Excel file
        /// </summary>
        /// <param name="file">Excel file</param>
        /// <returns>Import result</returns>
        Task<Acknowledgement<CustomerExcelImportResult>> ImportCustomersFromExcel(IFormFile file);

        /// <summary>
        /// Generate Excel template for customer import
        /// </summary>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> GenerateCustomerExcelTemplate();
    }
}
