using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Services;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.AuthorPredicates;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.DBContexts;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;

namespace Tasin.Website.DAL.Services.WebServices
{
    public class UnitService : BaseService<UnitService>, IUnitService
    {
        private readonly IMapper _mapper;
        private IUnitRepository _unitRepository;

        public UnitService(
            ILogger<UnitService> logger,
            IUserRepository userRepository,
            IUnitRepository unitRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _unitRepository = unitRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<UnitViewModel>>>> GetUnitList(UnitSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<UnitViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Unit>(i => i.IsActive == true);

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => i.NameNonUnicode.ToLower().Contains(searchStringNonUnicode) ||
                                                    i.Code.ToLower().Contains(searchStringNonUnicode)
                                             );
                }

                // Add author predicate if needed
                predicate = UnitAuthorPredicate.GetUnitAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var unitQuery = await _unitRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var unitViewModels = _mapper.Map<List<UnitViewModel>>(unitQuery.Data);

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                foreach (var unit in unitViewModels)
                {
                    if (unit.CreatedBy > 0 && !userIdList.Contains(unit.CreatedBy))
                        userIdList.Add(unit.CreatedBy);

                    if (unit.UpdatedBy.HasValue && unit.UpdatedBy.Value > 0 && !userIdList.Contains(unit.UpdatedBy.Value))
                        userIdList.Add(unit.UpdatedBy.Value);
                }

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                foreach (var unit in unitViewModels)
                {
                    var createdByUser = userList.FirstOrDefault(i => i.Id == unit.CreatedBy);
                    var updatedByUser = userList.FirstOrDefault(i => i.Id == unit.UpdatedBy);

                    unit.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "";
                }

                response.Data = new JsonResultPaging<List<UnitViewModel>>
                {
                    Data = unitViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = unitQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetUnitList: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<UnitViewModel>> GetUnitById(int unitId)
        {
            var ack = new Acknowledgement<UnitViewModel>();
            try
            {
                var unit = await _unitRepository.ReadOnlyRespository.FindAsync(unitId);
                if (unit == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy đơn vị");
                    return ack;
                }

                var unitViewModel = _mapper.Map<UnitViewModel>(unit);

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                if (unit.CreatedBy > 0)
                    userIdList.Add(unit.CreatedBy);

                if (unit.UpdatedBy.HasValue && unit.UpdatedBy.Value > 0)
                    userIdList.Add(unit.UpdatedBy.Value);

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                var createdByUser = userList.FirstOrDefault(i => i.Id == unit.CreatedBy);
                var updatedByUser = userList.FirstOrDefault(i => i.Id == unit.UpdatedBy);

                unitViewModel.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "Unknown";

                ack.Data = unitViewModel;
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"GetUnitById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteUnitById(int unitId)
        {
            var ack = new Acknowledgement();
            try
            {
                var unit = await _unitRepository.Repository.FindAsync(unitId);
                if (unit == null)
                {
                    ack.AddMessage("Không tìm thấy đơn vị.");
                    return ack;
                }

                unit.IsActive = false;
                unit.UpdatedDate = DateTime.Now;
                unit.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(unit), _unitRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"DeleteUnitById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateUnit(UnitViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(postData.Name))
                {
                    ack.AddMessage("Tên đơn vị không được để trống.");
                    return ack;
                }

                if (postData.Id == 0)
                {
                    var newUnit = _mapper.Map<Unit>(postData);
                    newUnit.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Unit, DbContext);
                    newUnit.NameNonUnicode = Utils.NonUnicode(newUnit.Name);
                    newUnit.CreatedDate = DateTime.Now;
                    newUnit.CreatedBy = CurrentUserId;
                    newUnit.UpdatedDate = newUnit.CreatedDate;
                    newUnit.UpdatedBy = newUnit.CreatedBy;
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newUnit), _unitRepository.Repository);
                }
                else
                {
                    var existingUnit = await _unitRepository.Repository.FindAsync(postData.Id);
                    if (existingUnit == null)
                    {
                        ack.AddMessage("Không tìm thấy đơn vị.");
                        return ack;
                    }

                    existingUnit.Name = postData.Name;
                    existingUnit.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existingUnit.Name_EN = postData.Name_EN;
                    existingUnit.Description = postData.Description;
                    existingUnit.UpdatedDate = DateTime.Now;
                    existingUnit.UpdatedBy = CurrentUserId;

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingUnit), _unitRepository.Repository);
                }

                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateOrUpdateUnit: {ex.Message}");
                return ack;
            }
        }
    }
}