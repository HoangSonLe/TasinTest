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
    public class VendorService : BaseService<VendorService>, IVendorService
    {
        private readonly IMapper _mapper;
        private IVendorRepository _vendorRepository;

        public VendorService(
            ILogger<VendorService> logger,
            IUserRepository userRepository,
            IVendorRepository vendorRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _vendorRepository = vendorRepository;
        }

        public async Task<Acknowledgement<JsonResultPaging<List<VendorViewModel>>>> GetVendorList(VendorSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<VendorViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Vendor>(i => i.IsActive == true);

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => i.NameNonUnicode.ToLower().Contains(searchStringNonUnicode) ||
                                                    i.Code.ToLower().Contains(searchStringNonUnicode)
                                             );
                }

                predicate = VendorAuthorPredicate.GetVendorAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);
                var vendorQuery = await _vendorRepository.ReadOnlyRespository.GetWithPagingAsync(
                    filter:predicate,
                    orderBy: q => q.OrderByDescending(u => u.UpdatedDate),
                    paging:new PagingParameters(searchModel.PageNumber, searchModel.PageSize)
                );

                var vendorViewModels = _mapper.Map<List<VendorViewModel>>(vendorQuery.Data);

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                foreach (var vendor in vendorViewModels)
                {
                    if (vendor.CreatedBy > 0 && !userIdList.Contains(vendor.CreatedBy))
                        userIdList.Add(vendor.CreatedBy);

                    if (vendor.UpdatedBy.HasValue && vendor.UpdatedBy.Value > 0 && !userIdList.Contains(vendor.UpdatedBy.Value))
                        userIdList.Add(vendor.UpdatedBy.Value);
                }

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                foreach (var vendor in vendorViewModels)
                {
                    var createdByUser = userList.FirstOrDefault(i => i.Id == vendor.CreatedBy);
                    var updatedByUser = userList.FirstOrDefault(i => i.Id == vendor.UpdatedBy);

                    vendor.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "";
                }

                response.Data = new JsonResultPaging<List<VendorViewModel>>
                {
                    Data = vendorViewModels,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = vendorQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError($"GetVendorList: {ex.Message}");
                return response;
            }
        }

        public async Task<Acknowledgement<VendorViewModel>> GetVendorById(int vendorId)
        {
            var ack = new Acknowledgement<VendorViewModel>();
            try
            {
                var vendor = await _vendorRepository.ReadOnlyRespository.FindAsync(vendorId);
                if (vendor == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy nhà cung cấp");
                    return ack;
                }

                var vendorViewModel = _mapper.Map<VendorViewModel>(vendor);

                // Get user names for created by and updated by
                var userIdList = new List<int>();
                if (vendor.CreatedBy > 0)
                    userIdList.Add(vendor.CreatedBy);

                if (vendor.UpdatedBy.HasValue && vendor.UpdatedBy.Value > 0)
                    userIdList.Add(vendor.UpdatedBy.Value);

                var userList = await _userRepository.ReadOnlyRespository.GetAsync(i => userIdList.Contains(i.Id));

                var createdByUser = userList.FirstOrDefault(i => i.Id == vendor.CreatedBy);
                var updatedByUser = userList.FirstOrDefault(i => i.Id == vendor.UpdatedBy);

                vendorViewModel.UpdatedByName = updatedByUser?.Name ?? createdByUser?.Name ?? "Unknown";

                ack.Data = vendorViewModel;
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"GetVendorById: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> CreateOrUpdateVendor(VendorViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (string.IsNullOrWhiteSpace(postData.Name))
                {
                    ack.AddMessage("Tên nhà cung cấp không được để trống.");
                    return ack;
                }

                if (postData.Id == 0)
                {
                    var newVendor = _mapper.Map<Vendor>(postData);
                    newVendor.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Vendor, DbContext);
                    newVendor.NameNonUnicode = Utils.NonUnicode(newVendor.Name);
                    newVendor.CreatedDate = DateTime.Now;
                    newVendor.CreatedBy = CurrentUserId;
                    newVendor.UpdatedDate = newVendor.CreatedDate;
                    newVendor.UpdatedBy = newVendor.CreatedBy;
                    await ack.TrySaveChangesAsync(res => res.AddAsync(newVendor), _vendorRepository.Repository);
                }
                else
                {
                    var existingVendor = await _vendorRepository.Repository.FindAsync(postData.Id);
                    if (existingVendor == null)
                    {
                        ack.AddMessage("Không tìm thấy nhà cung cấp.");
                        return ack;
                    }

                    existingVendor.Name = postData.Name;
                    existingVendor.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existingVendor.Address = postData.Address;
                    existingVendor.UpdatedDate = DateTime.Now;
                    existingVendor.UpdatedBy = CurrentUserId;

                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existingVendor), _vendorRepository.Repository);
                }

                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"CreateOrUpdateVendor: {ex.Message}");
                return ack;
            }
        }

        public async Task<Acknowledgement> DeleteVendorById(int vendorId)
        {
            var ack = new Acknowledgement();
            try
            {
                var vendor = await _vendorRepository.Repository.FindAsync(vendorId);
                if (vendor == null)
                {
                    ack.AddMessage("Không tìm thấy nhà cung cấp.");
                    return ack;
                }

                vendor.IsActive = false;
                vendor.UpdatedDate = DateTime.Now;
                vendor.UpdatedBy = CurrentUserId;

                await ack.TrySaveChangesAsync(res => res.UpdateAsync(vendor), _vendorRepository.Repository);
                return ack;
            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError($"DeleteVendorById: {ex.Message}");
                return ack;
            }
        }
    }
}
