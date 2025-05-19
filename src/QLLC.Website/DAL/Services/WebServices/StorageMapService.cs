using AutoMapper;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.CommonModels.BaseModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Helper;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.SearchModels;
using Tasin.Website.Models.ViewModels;
using Tasin.Website.Models.ViewModels.AccountViewModels;


namespace Tasin.Website.DAL.Services.WebServices
{
    public class StorageMapService : BaseService<StorageMapService>, IStorageMapService
    {
        private readonly IMapper _mapper;
        private readonly IStorageMapRepository _storageMapRepository;
        public StorageMapService(
            ILogger<StorageMapService> logger,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IStorageMapRepository storageMapRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor)
        {
            _mapper = mapper;
            _storageMapRepository = storageMapRepository;
        }
        public async Task<Acknowledgement<StorageMapViewModel>> GetStorageMapById(long storageMapId)
        {
            var ack = new Acknowledgement<StorageMapViewModel>();
            try
            {

                var urn = await _storageMapRepository.ReadOnlyRespository.FindAsync(storageMapId);
                if (urn == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessage("Không tìm thấy sơ đồ lưu trữ hủ cốt");
                }
                var responseData = _mapper.Map<StorageMapViewModel>(urn);
                ack.Data = responseData;
                ack.IsSuccess = true;
                return ack;
            }
            catch (Exception ex)
            {
                _logger.LogError("StorageMap GetStorageMapById " + ex.Message);
                ack.IsSuccess = false;
                return ack;
            }
        }
        public async Task<Acknowledgement> CreateOrUpdateStorageMap(StorageMapViewModel postData)
        {
            var ack = new Acknowledgement();
            try
            {
                if (postData.Id == 0)
                {
                    var newStorageMap = _mapper.Map<StorageMap>(postData);
                    newStorageMap.LocationNonUnicode = Utils.NonUnicode(newStorageMap.Location);
                    newStorageMap.Image = postData.Image;//
                    newStorageMap.TenantId = _currentTenantId.Value;
                    newStorageMap.CreatedDate = DateTime.Now;
                    newStorageMap.CreatedBy = _currentUserId;
                    newStorageMap.UpdatedDate = newStorageMap.CreatedDate;
                    newStorageMap.UpdatedBy = newStorageMap.CreatedBy;

                    await ack.TrySaveChangesAsync(res => res.AddAsync(newStorageMap), _storageMapRepository.Repository);
                }
                else
                {
                    var updateItem = await _storageMapRepository.Repository.FindAsync(postData.Id);
                    if (updateItem == null)
                    {
                        ack.AddMessage("Không tìm thấy linh cốt");
                        ack.IsSuccess = false;
                        return ack;
                    }
                    else
                    {
                        if(updateItem.Location != postData.Location)
                        {
                            updateItem.LocationNonUnicode = Utils.NonUnicode(postData.Location);
                            updateItem.Location = postData.Location;
                        }

                        updateItem.Description = postData.Description;
                        if (updateItem.Image != postData.Image && postData.Image != null)
                        {
                            updateItem.Image = postData.Image;
                        }
                        updateItem.UpdatedDate = DateTime.Now;
                        updateItem.UpdatedBy = _currentUserId;

                        await ack.TrySaveChangesAsync(res => res.UpdateAsync(updateItem), _storageMapRepository.Repository);
                    }
                }
                return ack;
            }
            catch (Exception ex)
            {
                _logger.LogError("StorageMap CreateOrUpdateStorageMap " + ex.Message);
                ack.IsSuccess = false;
                return ack;

            }
        }

        public async Task<Acknowledgement> DeleteStorageMapById(long storageMapId)
        {
            var ack = new Acknowledgement();
            try
            {
                var urn = await _storageMapRepository.Repository.FindAsync(storageMapId);
                if (urn == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessage("Không tìm thấy sơ đồ lưu trữ hủ cốt");
                    return ack;
                }
                urn.State = (int)EState.Delete;
                await ack.TrySaveChangesAsync(res => res.UpdateAsync(urn), _storageMapRepository.Repository);
                return ack;

            }
            catch (Exception ex)
            {
                _logger.LogError("StorageMap DeleteStorageMapById " + ex.Message);
                ack.IsSuccess = false;
                return ack;

            }
        }

        public async Task<Acknowledgement<JsonResultPaging<List<StorageMapViewModel>>>> GetStorageMapList(StorageMapSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<StorageMapViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<StorageMap>(e=>e.State==(short)EState.Active);

                if (_currentUserRoleId.Contains(ERoleType.Admin))
                {
                    predicate.And(i => i.State == (short)EState.Active && i.TenantId == _currentTenantId);
                }

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString);
                    predicate = predicate.And(i => (i.LocationNonUnicode.Contains(searchStringNonUnicode)
                                                    )
                                             );
                }


                var dbList = await _storageMapRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(searchModel.PageNumber, searchModel.PageSize),
                    predicate,
                    i => i.OrderByDescending(p => p.UpdatedDate)
                    );
                var data = _mapper.Map<List<StorageMapViewModel>>(dbList.Data);
                var prefixUrl = Configuration.GetSection("FTP:MediaServer").Value;
                data.ForEach(i =>
                {
                    if (!string.IsNullOrWhiteSpace(i.Image))
                    {
                        i.Image = prefixUrl + i.Image;
                    }
                    else
                    {
                        i.Image = "../content/images/noImage.png";
                    }
                });
                response.Data = new JsonResultPaging<List<StorageMapViewModel>>()
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
