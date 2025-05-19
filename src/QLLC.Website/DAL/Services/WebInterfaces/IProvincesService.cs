using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IProvincesService : IBaseService, IDisposable
    {
        Task<Acknowledgement<List<DropdownListModel>>> GetUserDataDropdownList(string searchString);

    }
}
