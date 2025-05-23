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
    public class SpecialProductTaxRateService : BaseService<SpecialProductTaxRateService>, ISpecialProductTaxRateService
    {
        private readonly IMapper _mapper;
        private ISpecialProductTaxRateRepository _specialProductTaxRateRepository;

        public SpecialProductTaxRateService(
            ILogger<SpecialProductTaxRateService> logger,
            IUserRepository userRepository,
            ISpecialProductTaxRateRepository specialProductTaxRateRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _specialProductTaxRateRepository = specialProductTaxRateRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<SpecialProductTaxRateViewModel>>>> GetSpecialProductTaxRateList(SpecialProductTaxRateSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<SpecialProductTaxRateViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<SpecialProductTaxRate>(i => i.IsActive == true);

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => i.NameNonUnicode.ToLower().Contains(searchStringNonUnicode) ||
                                                    i.Code.ToLower().Contains(searchStringNonUnicode)
                                             );
                }

                // Add author predicate if needed
                predicate = SpecialProductTaxRateAuthorPredicate.GetSpecialProductTaxRateAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                var taxRateQuery = await _specialProductTaxRateRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter: predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging: new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var specialProductTaxRateViewModels = _mapper.Map<List<SpecialProductTaxRateViewModel>>(taxRateQuery.Data);

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                foreach (var taxRate in specialProductTaxRateViewModels)
                {
                    if (taxRate.CreatedBy > 0 && !userIdList.Contains(taxRate.CreatedBy))
                        userIdList.Add(taxRate.CreatedBy);

                    if (taxRate.UpdatedBy.HasValue && taxRate.UpdatedBy.Value > 0 && !userIdList.Contains(taxRate.UpdatedBy.Value))
                        userIdList.Add(taxRate.UpdatedBy.Value);
                }

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                foreach (var taxRate in specialProductTaxRateViewModels)
                {
                    var createdByUser = userList.FirstOrDefault(i => i.Id == taxRate.CreatedBy);
                    var updatedByUser = userList.FirstOrDefault(i => i.Id == taxRate.UpdatedBy);

                    taxRate.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "";
                }

                response.Data = new JsonResultPaging<List<SpecialProductTaxRateViewModel>>
                {
                    Data = specialProductTaxRateViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = taxRateQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetSpecialProductTaxRateList: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<SpecialProductTaxRateViewModel>> GetSpecialProductTaxRateById(int specialProductTaxRateId)
        {
            var ack = new Acknowledgement<SpecialProductTaxRateViewModel>();
            try
            {
                var specialProductTaxRate = await _specialProductTaxRateRepository.ReadOnlyRespository.FindAsync(specialProductTaxRateId);
                if (specialProductTaxRate == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy thuế suất sản phẩm đặc biệt");
                    return ack;
                }

                var specialProductTaxRateViewModel = _mapper.Map<SpecialProductTaxRateViewModel>(specialProductTaxRate);

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                if (specialProductTaxRate.CreatedBy > 0)
                    userIdList.Add(specialProductTaxRate.CreatedBy);

                if (specialProductTaxRate.UpdatedBy.HasValue && specialProductTaxRate.UpdatedBy.Value > 0)
                    userIdList.Add(specialProductTaxRate.UpdatedBy.Value);

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                var createdByUser = userList.FirstOrDefault(i => i.Id == specialProductTaxRate.CreatedBy);
                var updatedByUser = userList.FirstOrDefault(i => i.Id == specialProductTaxRate.UpdatedBy);

                specialProductTaxRateViewModel.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "Unknown";

                ack.Data = specialProductTaxRateViewModel;
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"GetSpecialProductTaxRateById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteSpecialProductTaxRateById(int specialProductTaxRateId)
        {
            var ack = new Acknowledgement();
            try
            {
                var specialProductTaxRate = await _specialProductTaxRateRepository.Repository.FindAsync(specialProductTaxRateId);
                if (specialProductTaxRate == null)
                {
                    ack.AddMessage("Không tìm thấy thuế suất sản phẩm đặc biệt.");
                    return ack;
                }

                specialProductTaxRate.IsActive = false;
                specialProductTaxRate.UpdatedDate = DateTime.Now;
                specialProductTaxRate.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(specialProductTaxRate), _specialProductTaxRateRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"DeleteSpecialProductTaxRateById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateSpecialProductTaxRate(SpecialProductTaxRateViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(postData.Name))
                {
                    ack.AddMessage("Tên thuế suất sản phẩm đặc biệt không được để trống.");
                    return ack;
                }

                if (postData.Id == 0)
                {
                    var newSpecialProductTaxRate = _mapper.Map<SpecialProductTaxRate>(postData);
                    newSpecialProductTaxRate.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.SpecialProductTaxRate, DbContext);
                    newSpecialProductTaxRate.NameNonUnicode = Utils.NonUnicode(newSpecialProductTaxRate.Name);
                    newSpecialProductTaxRate.CreatedDate = DateTime.Now;
                    newSpecialProductTaxRate.CreatedBy = CurrentUserId;
                    newSpecialProductTaxRate.UpdatedDate = newSpecialProductTaxRate.CreatedDate;
                    newSpecialProductTaxRate.UpdatedBy = newSpecialProductTaxRate.CreatedBy;
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newSpecialProductTaxRate), _specialProductTaxRateRepository.Repository);
                }
                else
                {
                    var existingSpecialProductTaxRate = await _specialProductTaxRateRepository.Repository.FindAsync(postData.Id);
                    if (existingSpecialProductTaxRate == null)
                    {
                        ack.AddMessage("Không tìm thấy thuế suất sản phẩm đặc biệt.");
                        return ack;
                    }

                    existingSpecialProductTaxRate.Name = postData.Name;
                    existingSpecialProductTaxRate.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existingSpecialProductTaxRate.Name_EN = postData.Name_EN;
                    existingSpecialProductTaxRate.Description = postData.Description;
                    existingSpecialProductTaxRate.UpdatedDate = DateTime.Now;
                    existingSpecialProductTaxRate.UpdatedBy = CurrentUserId;

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingSpecialProductTaxRate), _specialProductTaxRateRepository.Repository);
                }

                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateOrUpdateSpecialProductTaxRate: {ex.Message}");
                return ack;
            }
        }
    }
}
