using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for vendors
    /// </summary>
    public class VendorSearchModel : SearchPagingModel<VendorViewModel>
    {
        // SearchString is inherited from SearchPagingModel
    }
}
