using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface ISpecialProductTaxRateService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<SpecialProductTaxRateViewModel>>>> GetSpecialProductTaxRateList(SpecialProductTaxRateSearchModel postData);
        Task<Acknowledgement<SpecialProductTaxRateViewModel>> GetSpecialProductTaxRateById(int specialProductTaxRateId);
        Task<Acknowledgement> CreateOrUpdateSpecialProductTaxRate(SpecialProductTaxRateViewModel postData);
        Task<Acknowledgement> DeleteSpecialProductTaxRateById(int specialProductTaxRateId);
    }
}
