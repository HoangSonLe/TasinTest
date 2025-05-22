using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface ICategoryService : IBaseService, IDisposable
    {
        Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetDataOptionsDropdown(string? searchString, ECategoryType type);

    }
}
