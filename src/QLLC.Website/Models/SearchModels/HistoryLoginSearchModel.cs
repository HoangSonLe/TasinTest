using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    public class HistoryLoginSearchModel : SearchPagingModel<HistoryLoginSearchModel>
    {
        public string SearchString { get; set; }
    }
}
