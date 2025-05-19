using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;


namespace Tasin.Website.DAL.Services.WebServices
{
    public class ReminderService : BaseService<ReminderService>, IReminderService
    {
        private readonly IMapper _mapper;
        private readonly IReminderRepository _reminderRepository;
        public ReminderService(
            ILogger<ReminderService> logger,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IReminderRepository reminderRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor)
        {
            _mapper = mapper;
            _reminderRepository = reminderRepository;
        }

        public async Task<Acknowledgement<ReminderViewModel>> GetReminderById(long reminderId)
        {
            var ack = new Acknowledgement<ReminderViewModel>();
            try
            {
                var urn = await _reminderRepository.ReadOnlyRespository.FindAsync(reminderId);
                if (urn == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessage("Không tìm thấy nhắc nhở");
                }
                var responseData = _mapper.Map<ReminderViewModel>(urn);
                ack.Data = responseData;
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                _logger.LogError("Reminder GetReminderById " + ex.Message);
                ack.IsSuccess = false;
                return ack;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateReminder(ReminderViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (postData.Id == 0)
                {
                    postData.UserId = _currentUserId;
                    var newUrn = _mapper.Map<Reminder>(postData);
                    newUrn.CreatedBy = _currentUserId;
                    newUrn.UpdatedBy = _currentUserId;
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newUrn), _reminderRepository.Repository);
                }
                else
                {
                    var updateItem = await _reminderRepository.Repository.FindAsync(postData.Id);
                    if (updateItem == null)
                    {
                        ack.AddMessage("Không tìm thấy nhắc nhở");
                        ack.IsSuccess = false;
                        return ack;
                    }
                    else
                    {
                        updateItem.UserId = postData.UserId;
                        updateItem.RemindDate = postData.RemindDate;
                        updateItem.Content = postData.Content;

                        await ack.TrySaveChangesAsync(res => res.UpdateAsync(updateItem), _reminderRepository.Repository);
                    }
                }
                return ack;
            }
            catch (Exception ex)
            {
                _logger.LogError("Reminder CreateOrUpdateReminder " + ex.Message);
                ack.IsSuccess = false;
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteReminderById(long reminderId)
        {
            var ack = new Acknowledgement();
            try
            {
                var urn = await _reminderRepository.Repository.FindAsync(reminderId);
                if (urn == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessage("Không tìm thấy nhắc nhở");
                    return ack;
                }
                urn.State = (int)EState.Delete;
                await ack.TrySaveChangesAsync(res => res.UpdateAsync(urn), _reminderRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                _logger.LogError("Reminder DeleteReminderById " + ex.Message);
                ack.IsSuccess = false;
                return ack;
            }
        }

        public async Task<Acknowledgement<JsonResultPaging<List<ReminderViewModel>>>> GetReminderList(ReminderSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<ReminderViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Reminder>(true);
                predicate = predicate.And(p => p.State == (short)EState.Active);
                //var isAdmin = _IsAdmin();
                //if (isAdmin == false)
                //{
                //    //predicate = predicate.And(i => i.FamilyMembers.Select(j => j.UserId).Contains(Int32.Parse(_currentUserId)));
                //}

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString);
                    predicate = predicate.And(i => (i.Content.Contains(searchStringNonUnicode)
                                                    )
                                             );
                }
                if (searchModel.FromDate.HasValue)
                {
                    predicate = predicate.And(i => i.RemindDate.Date >= searchModel.FromDate.Value.Date);
                }
                if (searchModel.ToDate.HasValue)
                {
                    predicate = predicate.And(i => i.RemindDate.Date <= searchModel.ToDate.Value.Date);
                }
                var dbList = await _reminderRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(searchModel.PageNumber, searchModel.PageSize),
                    predicate,
                    i => i.OrderByDescending(p => p.UpdatedDate),
                    "User"
                    );
                var data = _mapper.Map<List<ReminderViewModel>>(dbList.Data);

                response.Data = new JsonResultPaging<List<ReminderViewModel>>()
                {
                    Data = data,
                    PageNumber = searchModel.PageNumber,
                    PageSize = dbList.PageSize,
                    Total = dbList.TotalRecords
                };

                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Reminder GetReminderList " + ex.Message);
                response.IsSuccess = false;
                return response;
            }
        }
    }
}
