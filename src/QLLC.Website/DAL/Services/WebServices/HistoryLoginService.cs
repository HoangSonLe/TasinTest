using AutoMapper;
using LinqKit;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebServices
{
    public class HistoryLoginService : BaseService<HistoryLoginService>, IHistoryLoginService
    {
        private readonly IMapper _mapper;
        private readonly ITelegramChatRepository _telegramChatRepository;
        public HistoryLoginService(
            ILogger<HistoryLoginService> logger,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ITelegramChatRepository telegramChatRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor)
        {
            _mapper = mapper;
            _telegramChatRepository = telegramChatRepository;
        }
        public async Task<Acknowledgement<JsonResultPaging<List<HistoryLoginViewModel>>>> GetHistoryLoginList(HistoryLoginSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<HistoryLoginViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<TelegramChat>(e => e.State == (short)EState.Active);

                if (_currentUserRoleId.Contains(ERoleType.Admin))
                {
                    predicate.And(i => i.State == (short)EState.Active);
                }

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString);
                    predicate = predicate.And(i => (i.PhoneNumber.Contains(searchStringNonUnicode)
                                                    )
                                             );
                }

                var dbList = await _telegramChatRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(searchModel.PageNumber, searchModel.PageSize),
                    predicate,
                    i => i.OrderByDescending(p => p.UpdatedDate)
                    );
                var data = _mapper.Map<List<HistoryLoginViewModel>>(dbList.Data);
              
                response.Data = new JsonResultPaging<List<HistoryLoginViewModel>>()
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
                _logger.LogError("StorageMap GetStorageMapList " + ex.Message);
                response.IsSuccess = false;
                return response;

            }
        }

    }
}
