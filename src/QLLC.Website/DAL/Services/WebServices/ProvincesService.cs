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
using Tasin.Website.Models.ViewModels.AccountViewModels;


namespace Tasin.Website.DAL.Services.WebServices
{
    public class ProvincesService : BaseService<ProvincesService>, IProvincesService
    {
        private readonly IProvincesRepository _provincesRepository;
        public ProvincesService(
            ILogger<ProvincesService> logger,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IProvincesRepository provincesRepository,
            IConfiguration configuration
           
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor)
        {
           
            _provincesRepository = provincesRepository;
        }

        public async Task<Acknowledgement<List<DropdownListModel>>> GetUserDataDropdownList(string searchString)
        {
            var predicate = PredicateBuilder.New<Provinces>(true);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringNonUnicode = Utils.NonUnicode(searchString);
                predicate = predicate.And(i => (i.name_en.Contains(searchStringNonUnicode)
                                                )
                                         );
            }
            var provincesDbList = await _provincesRepository.ReadOnlyRespository.GetWithPagingAsync(new PagingParameters(1, 80), predicate, i => i.OrderBy(p => p.name));
            var data = provincesDbList.Data.Select(i => new DropdownListModel()
            {
                Code = i.code.ToString(),
                Name = i.name
            }).ToList();
            return new Acknowledgement<List<DropdownListModel>>()
            {
                IsSuccess = true,
                Data = data
            };
        }
    }
}
