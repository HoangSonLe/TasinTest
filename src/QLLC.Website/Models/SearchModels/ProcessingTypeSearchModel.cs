using System.ComponentModel.DataAnnotations;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    /// <summary>
    /// Search model for processing types
    /// </summary>
    public class ProcessingTypeSearchModel : SearchPagingModel<ProcessingTypeViewModel>
    {
        // SearchString is inherited from SearchPagingModel
    }
}
