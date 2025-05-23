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
    public class CategoryService : BaseService<CategoryService>, ICategoryService
    {
        private readonly IMapper _mapper;
        private ICategoryRepository _categoryRepository;

        public CategoryService(
            ILogger<CategoryService> logger,
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<CategoryViewModel>>>> GetCategoryList(CategorySearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<CategoryViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Category>(i => i.IsActive == true);

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
                predicate = CategoryAuthorPredicate.GetCategoryAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var categoryQuery = await _categoryRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var categoryViewModels = _mapper.Map<List<CategoryViewModel>>(categoryQuery.Data);

                // Add parent category names
                foreach (var category in categoryViewModels.Where(c => c.Parent_ID.HasValue))
                {
                    var parent = await _categoryRepository.ReadOnlyRespository.FindAsync(category.Parent_ID.Value);
                    if (parent != null)
                    {
                        category.ParentName = parent.Name;
                    }
                }

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                foreach (var category in categoryViewModels)
                {
                    if (category.CreatedBy > 0 && !userIdList.Contains(category.CreatedBy))
                        userIdList.Add(category.CreatedBy);

                    if (category.UpdatedBy.HasValue && category.UpdatedBy.Value > 0 && !userIdList.Contains(category.UpdatedBy.Value))
                        userIdList.Add(category.UpdatedBy.Value);
                }

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                foreach (var category in categoryViewModels)
                {
                    var createdByUser = userList.FirstOrDefault(i => i.Id == category.CreatedBy);
                    var updatedByUser = userList.FirstOrDefault(i => i.Id == category.UpdatedBy);

                    category.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "";
                }

                response.Data = new JsonResultPaging<List<CategoryViewModel>>
                {
                    Data = categoryViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = categoryQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetCategoryList: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<CategoryViewModel>> GetCategoryById(int categoryId)
        {
            var ack = new Acknowledgement<CategoryViewModel>();
            try
            {
                var category = await _categoryRepository.ReadOnlyRespository.FindAsync(categoryId);
                if (category == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy danh mục");
                    return ack;
                }

                var categoryViewModel = _mapper.Map<CategoryViewModel>(category);

                // Add parent category name
                if (category.Parent_ID.HasValue)
                {
                    var parent = await _categoryRepository.ReadOnlyRespository.FindAsync(category.Parent_ID.Value);
                    if (parent != null)
                    {
                        categoryViewModel.ParentName = parent.Name;
                    }
                }

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                if (category.CreatedBy > 0)
                    userIdList.Add(category.CreatedBy);

                if (category.UpdatedBy.HasValue && category.UpdatedBy.Value > 0)
                    userIdList.Add(category.UpdatedBy.Value);

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                var createdByUser = userList.FirstOrDefault(i => i.Id == category.CreatedBy);
                var updatedByUser = userList.FirstOrDefault(i => i.Id == category.UpdatedBy);

                categoryViewModel.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "Unknown";

                ack.Data = categoryViewModel;
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"GetCategoryById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteCategoryById(int categoryId)
        {
            var ack = new Acknowledgement();
            try
            {
                var category = await _categoryRepository.Repository.FindAsync(categoryId);
                if (category == null)
                {
                    ack.AddMessage("Không tìm thấy danh mục.");
                    return ack;
                }

                // Check if there are any child categories
                var hasChildren = await _categoryRepository.ReadOnlyRespository.FirstOrDefaultAsync(
                    i => i.Parent_ID == categoryId && i.IsActive == true
                );

                if (hasChildren != null)
                {
                    ack.AddMessage("Không thể xóa danh mục có danh mục con. Vui lòng xóa hoặc gán lại danh mục con trước.");
                    return ack;
                }

                category.IsActive = false;
                category.UpdatedDate = DateTime.Now;
                category.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(category), _categoryRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"DeleteCategoryById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateCategory(CategoryViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(postData.Name))
                {
                    ack.AddMessage("Tên danh mục không được để trống.");
                    return ack;
                }

                // Validate parent category if specified
                if (postData.Parent_ID.HasValue)
                {
                    var parentCategory = await _categoryRepository.ReadOnlyRespository.FindAsync(postData.Parent_ID.Value);

                    if (parentCategory == null)
                    {
                        ack.AddMessage("Danh mục cha không tồn tại.");
                        return ack;
                    }

                    // Prevent circular reference
                    if (postData.Id != 0 && postData.Id == postData.Parent_ID)
                    {
                        ack.AddMessage("Danh mục không thể là danh mục cha của chính nó.");
                        return ack;
                    }
                }

                if (postData.Id == 0)
                {
                    var newCategory = _mapper.Map<Category>(postData);
                    newCategory.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Category, DbContext);
                    newCategory.NameNonUnicode = Utils.NonUnicode(newCategory.Name);
                    newCategory.CreatedDate = DateTime.Now;
                    newCategory.CreatedBy = CurrentUserId;
                    newCategory.UpdatedDate = newCategory.CreatedDate;
                    newCategory.UpdatedBy = newCategory.CreatedBy;
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newCategory), _categoryRepository.Repository);
                }
                else
                {
                    var existingCategory = await _categoryRepository.Repository.FindAsync(postData.Id);
                    if (existingCategory == null)
                    {
                        ack.AddMessage("Không tìm thấy danh mục.");
                        return ack;
                    }

                    existingCategory.Name = postData.Name;
                    existingCategory.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existingCategory.Name_EN = postData.Name_EN;
                    existingCategory.Parent_ID = postData.Parent_ID;
                    existingCategory.Description = postData.Description;
                    existingCategory.UpdatedDate = DateTime.Now;
                    existingCategory.UpdatedBy = CurrentUserId;

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingCategory), _categoryRepository.Repository);
                }

                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateOrUpdateCategory: {ex.Message}");
                return ack;
            }
        }
    }
}
