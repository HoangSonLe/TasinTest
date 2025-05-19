using AutoMapper;
using LinqKit;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Services.AuthorPredicates;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using System.Data;


namespace Tasin.Website.DAL.Services.WebServices
{
    public class TenantService : BaseService<TenantService>, ITenantService
    {
        private readonly IMapper _mapper;
        private readonly ITenantRepository _tenantRepository;
        private readonly IUrnRepository _urnRepository;
        private readonly IConfigRepository _configRepository;
        public TenantService(
            ILogger<TenantService> logger,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ITenantRepository tenantRepository,
            IUrnRepository urnRepository,
            IConfigRepository configRepository,
            IConfiguration configuration,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor)
        {
            _mapper = mapper;
            _tenantRepository = tenantRepository;
            _urnRepository = urnRepository;
            _configRepository = configRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<TenantViewModel>>>> GetTenantList(UserSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<TenantViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Tenant>(true);

                if (_currentUserRoleId.Contains(ERoleType.Admin))
                {
                    predicate.And(i => i.State == (short)EState.Active && i.Id == _currentTenantId);
                }

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => (i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                                    i.AddressNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode)) ||
                                                    i.Code.Trim().ToLower().Contains(searchStringNonUnicode)
                                             );
                }

                var tennanList = new List<Tenant>();
                var tennantDbList = await _tenantRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(searchModel.PageNumber, searchModel.PageSize), 
                    predicate,
                    i=> i.OrderByDescending(u=> u.UpdatedDate)
                    );
                var data = _mapper.Map<List<TenantViewModel>>(tennantDbList.Data);
                response.Data = new JsonResultPaging<List<TenantViewModel>>()
                {
                    Data = data,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = tennantDbList.TotalRecords
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

        
        public async Task<Acknowledgement> CreateOrUpdate(TenantViewModel postData)
        {
            var ack = new Acknowledgement();
            var ack1 = new Acknowledgement();
            var defaultResetPassword = Configuration.GetSection("DefaultResetPassword").Value;
            var NumberOfDaysNoticeAnniversary = Configuration.GetSection("NumberOfDaysNoticeAnniversary").Value;
            var ReminderEmailSubject = Configuration.GetSection("ReminderEmailSubject").Value;
            var NumberOfDaysNoticeExpiredUrn = Configuration.GetSection("NumberOfDaysNoticeExpiredUrn").Value;
            var RemindNotification = Configuration.GetSection("RemindNotification").Value;
            var MonthGeneralNotification = Configuration.GetSection("MonthGeneralNotification").Value;
            var DayGeneralNotification = Configuration.GetSection("DayGeneralNotification").Value;

            if(postData.Name.Length > 70)
            {
                ack.AddMessage("Độ dài tên chùa quá dài");
                ack.IsSuccess = false;
                return ack;
            }

            if (Utils.ContainsSpecialCharacter(postData.Name))
            {
                ack.AddMessage("Tên chùa không được phép nhập ký tự đặc biệt");
                ack.IsSuccess = false;
                return ack;
            }


            if (postData.Id == 0)
            {
                var newTenant = _mapper.Map<Tenant>(postData);
                newTenant.NameNonUnicode = Utils.NonUnicode(newTenant.Name);
                newTenant.AddressNonUnicode = Utils.NonUnicode(newTenant.Address);
                //string templeCode = Generator.GenerateTenantCode(newTenant.NameNonUnicode);
                string finalCode = Generator.GenerateTenantCode(newTenant);
                newTenant.Code = finalCode;
                var checkCode = await _tenantRepository.ReadOnlyRespository.GetAsync(e => e.Code == finalCode && e.State == (short)EState.Active);
                if (checkCode.Count != 0)
                {
                    ack.AddMessage("Tên chùa tại tỉnh thành đã tồn tại");
                    ack.IsSuccess = false;
                    return ack;
                }
                newTenant.CreatedDate = DateTime.Now;
                newTenant.CreatedBy = _currentUserId;
                newTenant.UpdatedDate = newTenant.CreatedDate;
                newTenant.UpdatedBy = newTenant.CreatedBy;
                newTenant.Configs = new Config { 
                    NumberOfDaysNoticeAnniversary = Int32.Parse(NumberOfDaysNoticeAnniversary),
                    CreatedDate = DateTime.Now,
                    ReminderEmailSubject = ReminderEmailSubject,
                    UpdatedDate = DateTime.Now,
                    State = (short)EState.Active,
                    UpdatedBy = 1,
                    TenantName = newTenant.Name,
                    NumberOfDaysNoticeExpiredUrn = Int32.Parse(NumberOfDaysNoticeExpiredUrn),
                    RemindNotification = Int32.Parse(RemindNotification),
                    MonthGeneralNotification = Int32.Parse(MonthGeneralNotification),
                    DayGeneralNotification = Int32.Parse(DayGeneralNotification),
                    CreatedBy = 1, 
                };
                await ack.TrySaveChangesAsync(res => res.AddAsync(newTenant), _tenantRepository.Repository);
                var status = ack.IsSuccess;
                if (status) {
                    var userAdmin = new UserViewModel();
                    var newUser = _mapper.Map<User>(userAdmin);
                    newUser.Name = newTenant.Name;
                    newUser.NameNonUnicode = Utils.NonUnicode(newUser.Name);
                    newUser.UserName = Generator.UserNameGenerator("Admin", newTenant);
                    newUser.Phone = "";
                    newUser.TenantId = newTenant.Id;
                    newUser.CreatedBy = _currentUserId;
                    newUser.UpdatedDate = newTenant.UpdatedDate;
                    newUser.CreatedDate = newTenant.CreatedDate.Date;
                    newUser.RoleIdList = new List<int> { (int)ERoleType.Admin };
                    newUser.TypeAccount = 1;
                    newUser.Password = Utils.EncodePassword(newTenant.Code + defaultResetPassword, EEncodeType.SHA_256);
                   
                    await ack1.TrySaveChangesAsync(res => res.AddAsync(newUser), _userRepository.Repository);


                }
            }
            else
            {
                var updateItem = await _tenantRepository.Repository.FindAsync(postData.Id);
                if (updateItem == null)
                {
                    ack.AddMessage("Không tìm thấy chùa");
                    ack.IsSuccess = false;
                    return ack;
                }
                else
                {
                    updateItem.Id = postData.Id;
                    //updateItem.Name = postData.Name;
                    updateItem.Address = postData.Address;
                    //updateItem.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    updateItem.AddressNonUnicode = Utils.NonUnicode(postData.Address);
                    //string templeCode = GenerateTempleCode(updateItem.NameNonUnicode);
                    //string finalCode = $"{updateItem.Code.Split("_")[0]}_{templeCode}";
                    //updateItem.Code = finalCode;
                    updateItem.UpdatedDate = DateTime.Now;
                    updateItem.UpdatedBy = _currentUserId;
                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(updateItem), _tenantRepository.Repository);
                }
            }
            return ack;
        }
        public async Task<Acknowledgement> DeleteTenantById(int tenantId)
        {
            var ack = new Acknowledgement();
            var ack1 = new Acknowledgement();
            var ack2 = new Acknowledgement();
            if (tenantId > 0)
            {
                var urn = await _urnRepository.Repository.FirstOrDefaultAsync(e=>e.TenantId == tenantId);
                if (urn != null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessage("Chùa đã tạo linh cốt bạn không có quyền xóa!");
                    return ack;
                }

            }
           
            var tent = await _tenantRepository.Repository.FindAsync(tenantId);
            if (tent == null)
            {
                ack.IsSuccess = false;
                ack.AddMessage("Không tìm thấy chùa");
                return ack;
            }
            tent.State = (int)EState.Delete;
            //await ack.TrySaveChangesAsync(res => res.UpdateAsync(tent), _tenantRepository.Repository);
            await ack.TrySaveChangesAsync(res => res.DeleteAsync(tent.Id), _tenantRepository.Repository);
            var status = ack.IsSuccess;
            if (status)
            {
                //var urn = await _userRepository.Repository.FirstOrDefaultAsync(e => e.TenantId == tenantId);
                //await ack1.TrySaveChangesAsync(res => res.DeleteAsync(urn.Id), _userRepository.Repository);

                //var config = await _configRepository.Repository.FirstOrDefaultAsync(e => e.TenantId == tenantId);
                //await ack2.TrySaveChangesAsync(res => res.DeleteAsync(config.Id), _configRepository.Repository);
            }

            return ack;
        }
        public async Task<Acknowledgement<TenantViewModel>> GetUserById(int userId)
        {
            var ack = new Acknowledgement<TenantViewModel>();
            try
            {
                var isAdmin = _IsHasAdminRole();
                if(isAdmin == false && userId != _currentUserId)
                {
                    ack.IsSuccess = false;
                    ack.AddMessage("Bạn không có quyền xem thông tin của người dùng này");
                    return ack;
                }
                var user = await _userRepository.ReadOnlyRespository.FindAsync(userId);
                if (user == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy user");
                    return ack;
                }
                ack.Data = new TenantViewModel()
                {
                    Id = user.Id,
                    Name = user.Name,
                };
                ack.IsSuccess = true;
                return ack;

            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError("GetUserList " + ex.Message);
                return ack;
            }
        }
    }
}
