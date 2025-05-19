using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IRoleService : IBaseService, IDisposable
    {
        Task<Acknowledgement<List<KendoDropdownListModel<int>>>> GetRoleDropdownList(string searchString);

    }
}
