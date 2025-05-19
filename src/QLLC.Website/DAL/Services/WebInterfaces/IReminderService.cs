using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebInterfaces
{
    public interface IReminderService : IBaseService, IDisposable
    {
        Task<Acknowledgement<JsonResultPaging<List<ReminderViewModel>>>> GetReminderList(ReminderSearchModel searchModel);
        Task<Acknowledgement> CreateOrUpdateReminder(ReminderViewModel postData);
        Task<Acknowledgement> DeleteReminderById(long reminderId);
        Task<Acknowledgement<ReminderViewModel>> GetReminderById(long reminderId);

    }
}
