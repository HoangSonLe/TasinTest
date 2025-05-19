using AutoMapper;
using LinqKit;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;


namespace Tasin.Website.DAL.Services.WebServices
{
    public class ConfigService : BaseService<ConfigService>, IConfigService
    {
        private readonly IMapper _mapper;
        private readonly IConfigRepository _configRepository;
        private readonly IUrnRepository _urnRepository;
        public ConfigService(
            ILogger<ConfigService> logger,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfigRepository configRepository,
            IUrnRepository urnRepository,
            IConfiguration configuration,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor)
        {
            _mapper = mapper;
            _configRepository = configRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<ConfigViewModel>>>> GetConfigList(UserSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<ConfigViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Config>(true);
                predicate.And(i => i.State == (short)EState.Active);

                if (_currentUserRoleId.Contains(ERoleType.Admin))
                {
                    predicate.And(i => i.State == (short)EState.Active && i.TenantId == _currentTenantId);
                }

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString);
                    predicate = predicate.And(i => (i.ReminderEmailSubject.Contains(searchStringNonUnicode) ||
                                                    i.TenantName.Contains(searchStringNonUnicode))
                                             );
                }
                
                var userList = new List<Config>();
                var userDbList = await _configRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(searchModel.PageNumber, searchModel.PageSize), 
                    predicate,
                    i=> i.OrderByDescending(u=> u.UpdatedDate)
                    );
                var userDBList = _mapper.Map<List<ConfigViewModel>>(userDbList.Data);
                response.Data = new JsonResultPaging<List<ConfigViewModel>>()
                {
                    Data = userDBList,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = userDBList.Count
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError("GetUserList " + ex.Message);
                return response;

            }
        }

        public async Task<Acknowledgement> CreateOrUpdate(ConfigViewModel postData)
        {
            var ack = new Acknowledgement();
            var ack1 = new Acknowledgement();

                var updateItem = await _configRepository.Repository.FindAsync(postData.Id);
                if (updateItem == null)
                {
                    ack.AddMessage("Không tìm thấy thông tin config");
                    ack.IsSuccess = false;
                    return ack;
                }
                else
                {
                    updateItem.Id = postData.Id;
                    updateItem.NumberOfDaysNoticeAnniversary = postData.NumberOfDaysNoticeAnniversary;
                    updateItem.NumberOfDaysNoticeExpiredUrn = postData.NumberOfDaysNoticeExpiredUrn;
                    updateItem.RemindNotification = postData.RemindNotification;
                    updateItem.MonthGeneralNotification = postData.MonthGeneralNotification;
                    updateItem.DayGeneralNotification = postData.DayGeneralNotification;
                    updateItem.ReminderEmailSubject = postData.ReminderEmailSubject;
                    updateItem.UpdatedDate = DateTime.Now;
                    updateItem.UpdatedBy = _currentUserId;
                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(updateItem), _configRepository.Repository);
                }
            
            return ack;
        }


        public async Task<Acknowledgement<ConfigViewModel>> GetConfigByCurrentTenantId()
        {
            var ack = new Acknowledgement<ConfigViewModel>();
            var config = await _configRepository.ReadOnlyRespository.FirstOrDefaultAsync(p => p.TenantId == _currentTenantId);
            if (config == null)
            {
                ack.IsSuccess = false;
                ack.AddMessage("Không tìm thấy Cấu hình");
                return ack;
            }
            var responseData = _mapper.Map<ConfigViewModel>(config);
            ack.Data = responseData;
            ack.IsSuccess = true;
            return ack;
        }
    }
}
