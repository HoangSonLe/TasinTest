using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IVendorService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<VendorViewModel>>>> GetVendorList(VendorSearchModel postData);
        Task<Acknowledgement<VendorViewModel>> GetVendorById(int vendorId);
        Task<Acknowledgement> CreateOrUpdateVendor(VendorViewModel postData);
        Task<Acknowledgement> DeleteVendorById(int vendorId);
    }
}
