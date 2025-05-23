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
    public class MaterialService : BaseService<MaterialService>, IMaterialService
    {
        private readonly IMapper _mapper;
        private IMaterialRepository _materialRepository;

        public MaterialService(
            ILogger<MaterialService> logger,
            IUserRepository userRepository,
            IMaterialRepository materialRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _materialRepository = materialRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<MaterialViewModel>>>> GetMaterialList(MaterialSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<MaterialViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Material>(i => i.IsActive == true);

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => i.NameNonUnicode.ToLower().Contains(searchStringNonUnicode) ||
                                                    i.Code.ToLower().Contains(searchStringNonUnicode)
                                             );
                }

                if (searchModel.Parent_ID.HasValue)
                {
                    predicate = predicate.And(i => i.Parent_ID == searchModel.Parent_ID);
                }

                // Add author predicate if needed
                predicate = MaterialAuthorPredicate.GetMaterialAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var materialQuery = await _materialRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var materialViewModels = _mapper.Map<List<MaterialViewModel>>(materialQuery.Data);

                // Add parent material names
                foreach (var material in materialViewModels.Where(c => c.Parent_ID.HasValue))
                {
                    var parent = await _materialRepository.ReadOnlyRespository.FindAsync(material.Parent_ID.Value);
                    if (parent != null)
                    {
                        material.ParentName = parent.Name;
                    }
                }

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                foreach (var material in materialViewModels)
                {
                    if (material.CreatedBy > 0 && !userIdList.Contains(material.CreatedBy))
                        userIdList.Add(material.CreatedBy);

                    if (material.UpdatedBy.HasValue && material.UpdatedBy.Value > 0 && !userIdList.Contains(material.UpdatedBy.Value))
                        userIdList.Add(material.UpdatedBy.Value);
                }

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                foreach (var material in materialViewModels)
                {
                    var createdByUser = userList.FirstOrDefault(i => i.Id == material.CreatedBy);
                    var updatedByUser = userList.FirstOrDefault(i => i.Id == material.UpdatedBy);

                    material.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "";
                }

                response.Data = new JsonResultPaging<List<MaterialViewModel>>
                {
                    Data = materialViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = materialQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetMaterialList: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<MaterialViewModel>> GetMaterialById(int materialId)
        {
            var ack = new Acknowledgement<MaterialViewModel>();
            try
            {
                var material = await _materialRepository.ReadOnlyRespository.FindAsync(materialId);
                if (material == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy vật liệu");
                    return ack;
                }

                var materialViewModel = _mapper.Map<MaterialViewModel>(material);

                // Add parent material name
                if (material.Parent_ID.HasValue)
                {
                    var parent = await _materialRepository.ReadOnlyRespository.FindAsync(material.Parent_ID.Value);
                    if (parent != null)
                    {
                        materialViewModel.ParentName = parent.Name;
                    }
                }

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                if (material.CreatedBy > 0)
                    userIdList.Add(material.CreatedBy);

                if (material.UpdatedBy.HasValue && material.UpdatedBy.Value > 0)
                    userIdList.Add(material.UpdatedBy.Value);

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                var createdByUser = userList.FirstOrDefault(i => i.Id == material.CreatedBy);
                var updatedByUser = userList.FirstOrDefault(i => i.Id == material.UpdatedBy);

                materialViewModel.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "Unknown";

                ack.Data = materialViewModel;
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"GetMaterialById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteMaterialById(int materialId)
        {
            var ack = new Acknowledgement();
            try
            {
                var material = await _materialRepository.Repository.FindAsync(materialId);
                if (material == null)
                {
                    ack.AddMessage("Không tìm thấy vật liệu.");
                    return ack;
                }

                // Check if there are any child materials
                var hasChildren = await _materialRepository.ReadOnlyRespository.FirstOrDefaultAsync(
                    i => i.Parent_ID == materialId && i.IsActive == true
                );

                if (hasChildren != null)
                {
                    ack.AddMessage("Không thể xóa vật liệu có vật liệu con. Vui lòng xóa hoặc gán lại vật liệu con trước.");
                    return ack;
                }

                material.IsActive = false;
                material.UpdatedDate = DateTime.Now;
                material.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(material), _materialRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"DeleteMaterialById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateMaterial(MaterialViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(postData.Name))
                {
                    ack.AddMessage("Tên vật liệu không được để trống.");
                    return ack;
                }

                // Validate parent material if specified
                if (postData.Parent_ID.HasValue)
                {
                    var parentMaterial = await _materialRepository.ReadOnlyRespository.FindAsync(postData.Parent_ID.Value);

                    if (parentMaterial == null)
                    {
                        ack.AddMessage("Vật liệu cha không tồn tại.");
                        return ack;
                    }

                    // Prevent circular reference
                    if (postData.Id != 0 && postData.Id == postData.Parent_ID)
                    {
                        ack.AddMessage("Vật liệu không thể là vật liệu cha của chính nó.");
                        return ack;
                    }
                }

                if (postData.Id == 0)
                {
                    var newMaterial = _mapper.Map<Material>(postData);
                    newMaterial.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Material, DbContext);
                    newMaterial.NameNonUnicode = Utils.NonUnicode(newMaterial.Name);
                    newMaterial.CreatedDate = DateTime.Now;
                    newMaterial.CreatedBy = CurrentUserId;
                    newMaterial.UpdatedDate = newMaterial.CreatedDate;
                    newMaterial.UpdatedBy = newMaterial.CreatedBy;
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newMaterial), _materialRepository.Repository);
                }
                else
                {
                    var existingMaterial = await _materialRepository.Repository.FindAsync(postData.Id);
                    if (existingMaterial == null)
                    {
                        ack.AddMessage("Không tìm thấy vật liệu.");
                        return ack;
                    }

                    existingMaterial.Name = postData.Name;
                    existingMaterial.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existingMaterial.Name_EN = postData.Name_EN;
                    existingMaterial.Parent_ID = postData.Parent_ID;
                    existingMaterial.Description = postData.Description;
                    existingMaterial.UpdatedDate = DateTime.Now;
                    existingMaterial.UpdatedBy = CurrentUserId;

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingMaterial), _materialRepository.Repository);
                }

                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateOrUpdateMaterial: {ex.Message}");
                return ack;
            }
        }
    }
}
