using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for units
    /// </summary>
    public class UnitSearchModel : SearchPagingModel<UnitViewModel>
    {
        // SearchString is inherited from SearchPagingModel
    }
}
