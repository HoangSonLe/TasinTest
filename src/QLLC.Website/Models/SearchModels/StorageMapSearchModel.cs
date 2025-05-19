using Tasin.Website.Common.CommonModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.Models.SearchModels
{
    public class StorageMapSearchModel : SearchPagingModel<StorageMapViewModel>
    {
        public string SearchString { get; set; }
    }
}
