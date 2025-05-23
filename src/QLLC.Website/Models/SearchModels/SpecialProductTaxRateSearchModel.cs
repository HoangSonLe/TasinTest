using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for special product tax rates
    /// </summary>
    public class SpecialProductTaxRateSearchModel : SearchPagingModel<SpecialProductTaxRateViewModel>
    {
        // SearchString is inherited from SearchPagingModel
    }
}
