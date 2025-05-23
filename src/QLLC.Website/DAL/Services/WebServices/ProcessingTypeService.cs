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
    public class ProcessingTypeService : BaseService<ProcessingTypeService>, IProcessingTypeService
    {
        private readonly IMapper _mapper;
        private IProcessingTypeRepository _processingTypeRepository;

        public ProcessingTypeService(
            ILogger<ProcessingTypeService> logger,
            IUserRepository userRepository,
            IProcessingTypeRepository processingTypeRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _processingTypeRepository = processingTypeRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<ProcessingTypeViewModel>>>> GetProcessingTypeList(ProcessingTypeSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<ProcessingTypeViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<ProcessingType>(i => i.IsActive == true);

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => i.NameNonUnicode.ToLower().Contains(searchStringNonUnicode) ||
                                                    i.Code.ToLower().Contains(searchStringNonUnicode)
                                             );
                }

                // Add author predicate if needed
                predicate = ProcessingTypeAuthorPredicate.GetProcessingTypeAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var processingTypeQuery = await _processingTypeRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var processingTypeViewModels = _mapper.Map<List<ProcessingTypeViewModel>>(processingTypeQuery.Data);

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                foreach (var processingType in processingTypeViewModels)
                {
                    if (processingType.CreatedBy > 0 && !userIdList.Contains(processingType.CreatedBy))
                        userIdList.Add(processingType.CreatedBy);

                    if (processingType.UpdatedBy.HasValue && processingType.UpdatedBy.Value > 0 && !userIdList.Contains(processingType.UpdatedBy.Value))
                        userIdList.Add(processingType.UpdatedBy.Value);
                }

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                foreach (var processingType in processingTypeViewModels)
                {
                    var createdByUser = userList.FirstOrDefault(i => i.Id == processingType.CreatedBy);
                    var updatedByUser = userList.FirstOrDefault(i => i.Id == processingType.UpdatedBy);

                    processingType.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "";
                }

                response.Data = new JsonResultPaging<List<ProcessingTypeViewModel>>
                {
                    Data = processingTypeViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = processingTypeQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetProcessingTypeList: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<ProcessingTypeViewModel>> GetProcessingTypeById(int processingTypeId)
        {
            var ack = new Acknowledgement<ProcessingTypeViewModel>();
            try
            {
                var processingType = await _processingTypeRepository.ReadOnlyRespository.FindAsync(processingTypeId);
                if (processingType == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy loại chế biến");
                    return ack;
                }

                var processingTypeViewModel = _mapper.Map<ProcessingTypeViewModel>(processingType);

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                if (processingType.CreatedBy > 0)
                    userIdList.Add(processingType.CreatedBy);

                if (processingType.UpdatedBy.HasValue && processingType.UpdatedBy.Value > 0)
                    userIdList.Add(processingType.UpdatedBy.Value);

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                var createdByUser = userList.FirstOrDefault(i => i.Id == processingType.CreatedBy);
                var updatedByUser = userList.FirstOrDefault(i => i.Id == processingType.UpdatedBy);

                processingTypeViewModel.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "Unknown";

                ack.Data = processingTypeViewModel;
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"GetProcessingTypeById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteProcessingTypeById(int processingTypeId)
        {
            var ack = new Acknowledgement();
            try
            {
                var processingType = await _processingTypeRepository.Repository.FindAsync(processingTypeId);
                if (processingType == null)
                {
                    ack.AddMessage("Không tìm thấy loại chế biến.");
                    return ack;
                }

                processingType.IsActive = false;
                processingType.UpdatedDate = DateTime.Now;
                processingType.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(processingType), _processingTypeRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"DeleteProcessingTypeById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateProcessingType(ProcessingTypeViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(postData.Name))
                {
                    ack.AddMessage("Tên loại chế biến không được để trống.");
                    return ack;
                }

                if (postData.Id == 0)
                {
                    var newProcessingType = _mapper.Map<ProcessingType>(postData);
                    newProcessingType.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.ProcessingType, DbContext);
                    newProcessingType.NameNonUnicode = Utils.NonUnicode(newProcessingType.Name);
                    newProcessingType.CreatedDate = DateTime.Now;
                    newProcessingType.CreatedBy = CurrentUserId;
                    newProcessingType.UpdatedDate = newProcessingType.CreatedDate;
                    newProcessingType.UpdatedBy = newProcessingType.CreatedBy;
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newProcessingType), _processingTypeRepository.Repository);
                }
                else
                {
                    var existingProcessingType = await _processingTypeRepository.Repository.FindAsync(postData.Id);
                    if (existingProcessingType == null)
                    {
                        ack.AddMessage("Không tìm thấy loại chế biến.");
                        return ack;
                    }

                    existingProcessingType.Name = postData.Name;
                    existingProcessingType.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existingProcessingType.Name_EN = postData.Name_EN;
                    existingProcessingType.Description = postData.Description;
                    existingProcessingType.UpdatedDate = DateTime.Now;
                    existingProcessingType.UpdatedBy = CurrentUserId;

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingProcessingType), _processingTypeRepository.Repository);
                }

                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateOrUpdateProcessingType: {ex.Message}");
                return ack;
            }
        }
    }
}
