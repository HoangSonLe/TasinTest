using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Asn1.Ocsp;
using Tasin.Website.Authorizations;
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
using System.Data.Entity;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Security.AccessControl;

namespace Tasin.Website.DAL.Services.WebServices
{
    public class UrnService : BaseService<UrnService>, IUrnService
    {
        private readonly IMapper _mapper;
        private readonly IUrnRepository _urnRepository;
        private readonly IConfigRepository _configRepository;
        public UrnService(
            ILogger<UrnService> logger,
            IConfiguration configuration,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUrnRepository urnRepository,
            IConfigRepository configRepository,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper
            ) : base(logger,configuration, userRepository, roleRepository,httpContextAccessor)
        {
            _mapper = mapper;
            _urnRepository = urnRepository;
            _configRepository = configRepository;
        }
        public async Task<Acknowledgement<UrnViewModel>> GetUrnById(long urnId)
        {
            var ack = new Acknowledgement<UrnViewModel>();
            var urn = await _urnRepository.ReadOnlyRespository.FirstOrDefaultAsync(p=> p.Id == urnId, "FamilyMembers,FamilyMembers.User");
            if (urn == null)
            {
                ack.IsSuccess = false;
                ack.AddMessage("Không tìm thấy linh cốt");
                return ack;
            }
            var responseData = _mapper.Map<UrnViewModel>(urn);
            responseData.fileImageUrlWithLowQuality = GetImageFileUrlWithLowQuality(responseData.FileImageUrl);
            responseData.FileImageUrl = GetImageFileUrl(responseData.FileImageUrl);
            ack.Data = responseData;
            ack.IsSuccess = true;
            return ack;
        }
        public string GetImageFileUrlWithLowQuality(string rawUrl)
        {
            var prefixUrl = Configuration.GetSection("FTP:MediaServerWithLowQuality").Value;
            return prefixUrl + rawUrl;
        }
        public string GetImageFileUrl(string rawUrl)
        {
            var prefixUrl = Configuration.GetSection("FTP:MediaServer").Value;
            return prefixUrl + rawUrl;
        }
        public string ExtractImageFileRawUrl(string url)
        {
            var prefixUrl = Configuration.GetSection("FTP:MediaServer").Value;
            if (url.StartsWith(prefixUrl))
            {
                string rawUrl = url.Substring(prefixUrl.Length);
                return rawUrl;
            }
            return url;
        }
        public async Task<Acknowledgement> CreateOrUpdateUrn(UrnViewModel postData)
        {
            var ack = new Acknowledgement();

            if (postData.ExpiredDate.Date < DateTime.Now.Date)
            {
                ack.IsSuccess = false;
                ack.AddMessages("Hạn ký gửi không được nhỏ hơn ngày hiện tại.");
                return ack;
            }
            if (postData.BirthDate > DateTime.Now.Date)
            {
                ack.IsSuccess = false;
                ack.AddMessages("Ngày sinh không được lớn hơn ngày hiện tại.");
                return ack;
            }
            if (postData.DeathDate.Date > DateTime.Now.Date)
            {
                ack.IsSuccess = false;
                ack.AddMessages("Ngày mất không được lớn hơn ngày hiện tại.");
                return ack;
            }
            if (postData.DeathDate.Date < postData.BirthDate)
            {
                ack.IsSuccess = false;
                ack.AddMessages("Ngày mất không được nhỏ hơn ngày sinh.");
                return ack;
            }
            if (!VietCalendar.IsValidLunarDate(postData.BirthDate))
            {
                ack.IsSuccess = false;
                ack.AddMessages("Ngày sinh (ngày âm) không hợp lệ");
                return ack;
            }
            if (!VietCalendar.IsValidLunarDate(postData.DeathDate))
            {
                ack.IsSuccess = false;
                ack.AddMessages("Ngày mất (ngày âm) không hợp lệ");
                return ack;
            }
            var validateStringName = Validate.IsValidateName(postData.NameNonUnicode);
            if (validateStringName != null)
            {
                ack.IsSuccess = false;
                ack.AddMessages(validateStringName);
                return ack;
            }
            
            //if (string.IsNullOrEmpty(postData.TowerLocation))
            //{
            //    ack.IsSuccess = false;
            //    ack.AddMessages("Vị trí tháp không được để trống.");
            //    return ack;
            //}
            //if (string.IsNullOrEmpty(postData.CabinetName))
            //{
            //    ack.IsSuccess = false;
            //    ack.AddMessages("Tên tủ không được để trống.");
            //    return ack;
            //}
            if (postData.RowNumber < 0)
            {
                ack.IsSuccess = false;
                ack.AddMessages("Vui lòng nhập số hàng.");
                return ack;
            }
            if (postData.BoxNumber < 0)
            {
                ack.IsSuccess = false;
                ack.AddMessages("Vui lòng nhập số ô.");
                return ack;
            }

            //var checkSameLocation = await _urnRepository.ReadOnlyRespository.FirstOrDefaultAsync(i => i.TowerLocationNonUnicode == postData.TowerLocationNonUnicode &&
            //                                                                         i.CabinetName == postData.CabinetNameNonUnicode &&
            //                                                                         i.RowNumber == postData.RowNumber &&
            //                                                                         i.BoxNumber == postData.BoxNumber && i.State == (int)EState.Active
            //                                                                    );
            //if(checkSameLocation != null && (postData.Id == 0 || postData.Id != checkSameLocation.Id))
            //{
            //    ack.IsSuccess = false;
            //    ack.AddMessages("Vị trí này đã có linh cốt.");
            //    return ack;
            //}
            if (postData.FamilyMemberIdList.Count() == 0)
            {
                ack.IsSuccess = false;
                ack.AddMessages("Vui lòng chọn người ký gửi.");
                return ack;
            }

            if (postData.Id == 0)
            {
                var newUrn = _mapper.Map<Urn>(postData);
                newUrn.CreatedDate = DateTime.Now;
                newUrn.CreatedBy = _currentUserId;
                newUrn.UpdatedDate = newUrn.CreatedDate;
                newUrn.UpdatedBy = newUrn.CreatedBy;
                newUrn.TenantId = _currentTenantId.Value;

                foreach (var u in postData.FamilyMemberIdList)
                {
                    newUrn.FamilyMembers.Add(new User_Urn()
                    {
                        Urn = newUrn,
                        UserId = u.Value
                    });
                }
                await ack.TrySaveChangesAsync(res => res.AddAsync(newUrn), _urnRepository.Repository);
            }
            else
            {
                var updateItem = await _urnRepository.Repository.FirstOrDefaultAsync( p => p.Id == postData.Id, "FamilyMembers");
                if(updateItem == null)
                {
                    ack.AddMessage("Không tìm thấy linh cốt");
                    ack.IsSuccess = false;
                    return ack;
                }
                else
                {
                    updateItem.Name = postData.Name;
                    updateItem.NameNonUnicode = postData.NameNonUnicode;
                    updateItem.DharmaNameNonUnicode = postData.DharmaNameNonUnicode;
                    updateItem.CabinetNameNonUnicode = postData.CabinetNameNonUnicode;
                    updateItem.TowerLocationNonUnicode = "";
                    updateItem.DharmaName = postData.DharmaName;
                    updateItem.BirthDate = postData.BirthDate;
                    updateItem.DeathDate = postData.DeathDate;
                    updateItem.Gender = postData.Gender;
                    updateItem.UrnType = postData.UrnType;
                    updateItem.Note = postData.Note;
                    updateItem.TowerLocation = "";
                    updateItem.CabinetName = postData.CabinetName;
                    updateItem.RowNumber = postData.RowNumber;
                    updateItem.BoxNumber = postData.BoxNumber;
                    updateItem.LocationNumber = postData.LocationNumber;
                    updateItem.UpdatedDate = DateTime.Now;
                    updateItem.UpdatedBy = _currentUserId;
                    updateItem.ExpiredDate = postData.ExpiredDate;
                    var oldUrl = null as string;
                    if(postData.FileImageUrl != null)
                    {
                        postData.FileImageUrl = ExtractImageFileRawUrl(postData.FileImageUrl);
                        if(updateItem.FileImageUrl != postData.FileImageUrl)
                        {
                            oldUrl = updateItem.FileImageUrl;
                            updateItem.FileImageUrl = postData.FileImageUrl;
                        }
                    }
                    var leftJoin = (from u in updateItem.FamilyMembers
                               join n in postData.FamilyMemberIdList on u.UserId equals n into n1
                               from n in n1.DefaultIfEmpty()
                               select new { o = u, n }).ToList();
                    var rightJoin = (from n in postData.FamilyMemberIdList
                                join u in updateItem.FamilyMembers on n equals u.UserId into u1
                                from u in u1.DefaultIfEmpty()
                                select new { o = u, n }).ToList();
                    var join = leftJoin.Concat(rightJoin).Distinct().ToList();
                    foreach (var u in join)
                    {
                        if (u.n == null as int?)
                        {
                            updateItem.FamilyMembers.Remove(u.o);
                        }
                        if(u.o == null)
                        {
                            updateItem.FamilyMembers.Add(new User_Urn()
                            {
                                Urn = updateItem,
                                UserId = u.n.Value
                            });
                        }
                    }
                  
                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(updateItem), _urnRepository.Repository);
                    if (ack.IsSuccess && !string.IsNullOrEmpty(oldUrl))
                    {
                        var ackDelete = FtpFunction.DeleteFileInFolder(oldUrl);
                        if (!ackDelete.IsSuccess)
                        {
                            var errorMessages = ackDelete.ToException();
                            _logger.LogError(errorMessages.Message);
                        }
                    }
                }
            }
            return ack;
        }

        public async Task<Acknowledgement> DeleteUrnById(long urnId)
        {
            var ack = new Acknowledgement();
            var urn = await _urnRepository.Repository.FindAsync(urnId);
            if (urn == null)
            {
                ack.AddMessage("Không tìm thấy linh cốt");
                return ack;
            }
            if(urn.ExpiredDate.Date > DateTime.Now)
            {
                ack.AddMessage("Không thể xóa linh cốt vẫn còn hiệu lực ký gửi");
                return ack;
            }
            urn.State = (int)EState.Delete;
            var rawUrl = ExtractImageFileRawUrl(urn.FileImageUrl);
            await ack.TrySaveChangesAsync(res => res.UpdateAsync(urn), _urnRepository.Repository);
            if (ack.IsSuccess)
            {
                var ackDelete = FtpFunction.DeleteFileInFolder(rawUrl);
                if (!ackDelete.IsSuccess)
                {
                    var errorMessages = ackDelete.ToException();
                    _logger.LogError(errorMessages.Message);
                }
            }
            return ack;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<UrnViewModel>>>> GetUrnList(UrnSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<UrnViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Urn>(i => i.State == (int)EState.Active);
                #region Validate
                if (searchModel.FromBirthAndDeathDate.HasValue && searchModel.ToBirthAndDeathDate.HasValue && searchModel.FromBirthAndDeathDate > searchModel.ToBirthAndDeathDate)
                {
                    response.IsSuccess = false;
                    response.AddMessage("Ngày bắt đầu không được lớn hơn ngày kết thúc");
                    return response;
                }
                if (searchModel.FromExpiredDate.HasValue && searchModel.ToExpiredDate.HasValue && searchModel.FromExpiredDate > searchModel.ToExpiredDate)
                {
                    response.IsSuccess = false;
                    response.AddMessage("Ngày bắt đầu không được lớn hơn ngày kết thúc");
                    return response;
                }
                #endregion
                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => (
                                                    i.CabinetNameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                                    i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                                    i.DharmaNameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                                    i.BoxNumber.ToString().Contains(searchStringNonUnicode) ||
                                                    i.RowNumber.ToString().Contains(searchStringNonUnicode) ||
                                                    i.LocationNumber.ToString().Contains(searchStringNonUnicode) ||
                                                    i.Note.Contains(searchModel.SearchString)
                                                    )
                                             );
                }
                if (!string.IsNullOrEmpty(searchModel.SearchNoteString))
                {
                    predicate = predicate.And(i => i.Note.Contains(searchModel.SearchNoteString));
                }
                if (searchModel.Gender.HasValue)
                {
                    predicate = predicate.And(i => i.Gender == searchModel.Gender.Value);
                }
                if (searchModel.UrnType.HasValue)
                {
                    predicate = predicate.And(i => i.UrnType == searchModel.UrnType.Value);
                }
                if (searchModel.FromBirthAndDeathDate.HasValue)
                {
                    predicate = predicate.And(i => i.BirthDate.Date == searchModel.FromBirthAndDeathDate.Value.Date);
                }
                //if (searchModel.ToBirthAndDeathDate.HasValue)
                //{
                //    predicate = predicate.And(i => i.BirthDate.Date <= searchModel.ToBirthAndDeathDate.Value.Date);
                //}
                //if (searchModel.FromBirthAndDeathDate.HasValue)
                //{
                //    predicate = predicate.And(i => i.DeathDate.Date >= searchModel.FromBirthAndDeathDate.Value.Date);
                //}
                if (searchModel.ToBirthAndDeathDate.HasValue)
                {
                    predicate = predicate.And(i => i.DeathDate.Date == searchModel.ToBirthAndDeathDate.Value.Date);
                }
                if (searchModel.FromExpiredDate.HasValue)
                {
                    predicate = predicate.And(i => i.ExpiredDate.Date >= searchModel.FromExpiredDate.Value.Date);
                }
                if (searchModel.ToExpiredDate.HasValue)
                {
                    predicate = predicate.And(i => i.ExpiredDate.Date <= searchModel.ToExpiredDate.Value.Date);
                }
                predicate = UrnAuthorPredicate.GetUrnAuthorPredicate(predicate, _currentUserRoleId, _currentTenantId, _currentUserId);
                var dbList = await _urnRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(searchModel.PageNumber, searchModel.PageSize),
                    predicate,
                    i => i.OrderByDescending(p => p.UpdatedDate),
                    "FamilyMembers,FamilyMembers.User"
                    );

            var data = _mapper.Map<List<UrnViewModel>>(dbList.Data);
            var updatedByUserIdList = data.Select(i=> i.UpdatedBy).Distinct().ToList();
            var userDBList = (await _userRepository.ReadOnlyRespository.GetAsync(i=> updatedByUserIdList.Contains(i.Id))).ToDictionary(i=> i.Id, i => i.Name);
            data.ForEach(i =>
            {
                i.UpdatedByName = userDBList.TryGetValue(i.UpdatedBy, out var name) ? name : "";
                if (!string.IsNullOrWhiteSpace(i.FileImageUrl))
                {
                    i.fileImageUrlWithLowQuality = GetImageFileUrlWithLowQuality(i.FileImageUrl);
                    i.FileImageUrl = GetImageFileUrl(i.FileImageUrl);
                }
                else
                {
                    i.FileImageUrl = "../content/images/noImage.png";
                    i.fileImageUrlWithLowQuality = i.FileImageUrl;
                }
            });
           
                response.Data = new JsonResultPaging<List<UrnViewModel>>()
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

                _logger.LogError("Urn GetUrnList " + ex.Message);
                response.IsSuccess = false;
                return response;
            }
           

        }
        public async Task<Acknowledgement<JsonResultPaging<List<UrnViewModel>>>> GetUrnWorshipDayList(UrnSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<UrnViewModel>>>();
            try
            {
                var config = await _configRepository.ReadOnlyRespository.FirstOrDefaultAsync(i=> i.TenantId == _currentTenantId);

                var predicate = PredicateBuilder.New<Urn>(i => i.State == (int)EState.Active && i.TenantId == _currentTenantId);

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => (
                                                    i.CabinetNameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                                    i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                                    i.DharmaNameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                                    i.BoxNumber.ToString().Contains(searchStringNonUnicode) ||
                                                    i.RowNumber.ToString().Contains(searchStringNonUnicode) ||
                                                    i.LocationNumber.ToString().Contains(searchStringNonUnicode) ||
                                                    i.Note.Contains(searchModel.SearchString)
                                                    )
                                             );
                }

                var urnDB = await _urnRepository.ReadOnlyRespository.GetAsync( predicate, null, null, "FamilyMembers,FamilyMembers.User" );

                var urnWithLunarDate = _mapper.Map<List<UrnViewModel>>(urnDB);

                var now = DateTime.Now;
                var responseData = new List<UrnViewModel>();
                //Danh sách linh cốt cần nhắc giỗ

                

                if (searchModel.IsFilterAnniversary)
                {
                    responseData = urnWithLunarDate.Where(i => i.PreviewLunarDate.TotalDays > 0 && i.PreviewLunarDate.TotalDays <= config.NumberOfDaysNoticeAnniversary).ToList();
                }
                else
                {
                    //Danh sách linh cốt cần nhắc ký gửi
                    responseData = urnWithLunarDate.Where(i => now.Subtract(i.ExpiredDate).TotalDays <= config.NumberOfDaysNoticeExpiredUrn).ToList();

                }
                var total = responseData.Count();

                responseData = responseData.Skip((searchModel.PageNumber - 1) * searchModel.PageSize).Take(searchModel.PageSize).ToList();
                response.Data = new JsonResultPaging<List<UrnViewModel>>()
                {
                    Data = responseData,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = total,
                };

                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Urn GetUrnWorshipDayList " + ex.Message);
                response.IsSuccess = false;
                return response;
            }
           

        }
    }
}
