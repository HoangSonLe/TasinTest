using LinqKit;
using Microsoft.EntityFrameworkCore;
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

namespace Tasin.Website.DAL.Services.WebServices
{
    public class CommonService : BaseService<CommonService>, ICommonService
    {
        ICustomerRepository _customerRepository;
        IUnitRepository _unitRepository;
        IProcessingTypeRepository _processingTypeRepository;
        IMaterialRepository _materialRepository;
        IVendorRepository _vendorRepository;
        IProductRepository _productRepository;

        public CommonService(
            ILogger<CommonService> logger,
            IConfiguration configuration,
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IUnitRepository unitRepository,
            IProcessingTypeRepository processingTypeRepository,
            IMaterialRepository materialRepository,
            IVendorRepository vendorRepository,
            IProductRepository productRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _customerRepository = customerRepository;
            _unitRepository = unitRepository;
            _processingTypeRepository = processingTypeRepository;
            _materialRepository = materialRepository;
            _vendorRepository = vendorRepository;
            _productRepository = productRepository;
        }


        private List<KendoDropdownListModel<string>> GetCustomerType(string? searchString)
        {
            var options = EnumHelper.ToDropdownList<ECustomerType>();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                options = options.SearchWithoutDiacriticsInMemory(i => i.Text, searchString).ToList();
            }
            return options;
        }
        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetCustomerDataDropdownList(string searchString)
        {
            var predicate = PredicateBuilder.New<Customer>(i => i.IsActive == true);
            predicate = CustomerAuthorPredicate.GetCustomerAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);
            var selectedUserList = new List<User>();
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringNonUnicode = Utils.NonUnicode(searchString.Trim().ToLower());
                predicate = predicate.And(i => i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode));
            }
            var customerDBList = await _customerRepository.ReadOnlyRespository.GetWithPagingAsync(new PagingParameters(1, 50), predicate, i => i.OrderBy(p => p.Name));
            var data = customerDBList.Data.Select(i => new KendoDropdownListModel<string>()
            {
                Value = i.ID.ToString(),
                Text = $"{i.Name} - {i.PhoneContact}",
            }).ToList();
            return new Acknowledgement<List<KendoDropdownListModel<string>>>()
            {
                IsSuccess = true,
                Data = data
            };
        }

        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetUnitDataDropdownList(string searchString)
        {
            var predicate = PredicateBuilder.New<Unit>(i => i.IsActive == true);
            predicate = UnitAuthorPredicate.GetUnitAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringNonUnicode = Utils.NonUnicode(searchString.Trim().ToLower());
                predicate = predicate.And(i => i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                               i.Code.Trim().ToLower().Contains(searchStringNonUnicode));
            }
            var unitDBList = await _unitRepository.ReadOnlyRespository.GetWithPagingAsync(new PagingParameters(1, 50), predicate, i => i.OrderBy(p => p.Name));
            var data = unitDBList.Data.Select(i => new KendoDropdownListModel<string>()
            {
                Value = i.ID.ToString(),
                Text = $"{i.Name} ({i.Code})",
            }).ToList();
            return new Acknowledgement<List<KendoDropdownListModel<string>>>()
            {
                IsSuccess = true,
                Data = data
            };
        }

        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetProcessingTypeDataDropdownList(string searchString)
        {
            var predicate = PredicateBuilder.New<ProcessingType>(i => i.IsActive == true);
            predicate = ProcessingTypeAuthorPredicate.GetProcessingTypeAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringNonUnicode = Utils.NonUnicode(searchString.Trim().ToLower());
                predicate = predicate.And(i => i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                               i.Code.Trim().ToLower().Contains(searchStringNonUnicode));
            }
            var processingTypeDBList = await _processingTypeRepository.ReadOnlyRespository.GetWithPagingAsync(new PagingParameters(1, 50), predicate, i => i.OrderBy(p => p.Name));
            var data = processingTypeDBList.Data.Select(i => new KendoDropdownListModel<string>()
            {
                Value = i.ID.ToString(),
                Text = $"{i.Name} ({i.Code})",
            }).ToList();
            return new Acknowledgement<List<KendoDropdownListModel<string>>>()
            {
                IsSuccess = true,
                Data = data
            };
        }

        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetMaterialDataDropdownList(string searchString)
        {
            var predicate = PredicateBuilder.New<Material>(i => i.IsActive == true);
            predicate = MaterialAuthorPredicate.GetMaterialAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringNonUnicode = Utils.NonUnicode(searchString.Trim().ToLower());
                predicate = predicate.And(i => i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                               i.Code.Trim().ToLower().Contains(searchStringNonUnicode));
            }
            var materialDBList = await _materialRepository.ReadOnlyRespository.GetWithPagingAsync(new PagingParameters(1, 50), predicate, i => i.OrderBy(p => p.Name));
            var data = materialDBList.Data.Select(i => new KendoDropdownListModel<string>()
            {
                Value = i.ID.ToString(),
                Text = $"{i.Name} ({i.Code})",
            }).ToList();
            return new Acknowledgement<List<KendoDropdownListModel<string>>>()
            {
                IsSuccess = true,
                Data = data
            };
        }

        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetVendorDataDropdownList(string searchString)
        {
            var predicate = PredicateBuilder.New<Vendor>(i => i.IsActive == true);
            predicate = VendorAuthorPredicate.GetVendorAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringNonUnicode = Utils.NonUnicode(searchString.Trim().ToLower());
                predicate = predicate.And(i => i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                               i.Code.Trim().ToLower().Contains(searchStringNonUnicode));
            }
            var vendorDBList = await _vendorRepository.ReadOnlyRespository.GetWithPagingAsync(new PagingParameters(1, 50), predicate, i => i.OrderBy(p => p.Name));
            var data = vendorDBList.Data.Select(i => new KendoDropdownListModel<string>()
            {
                Value = i.ID.ToString(),
                Text = $"{i.Name} ({i.Code})",
            }).ToList();
            return new Acknowledgement<List<KendoDropdownListModel<string>>>()
            {
                IsSuccess = true,
                Data = data
            };
        }

        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetProductDataDropdownList(string searchString)
        {
            try
            {
                var predicate = PredicateBuilder.New<Product>(i => i.IsActive == true && !i.IsDiscontinued);
                predicate = ProductAuthorPredicate.GetProductAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);

                if (!string.IsNullOrEmpty(searchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchString.Trim().ToLower());
                    predicate = predicate.And(i =>
                        (i.NameNonUnicode != null && i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode)) ||
                        i.Code.Trim().ToLower().Contains(searchStringNonUnicode)
                    );
                }

                var productDBList = await _productRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(1, 50),
                    predicate,
                    i => i.OrderBy(p => p.Name)
                );

                var data = productDBList.Data.Select(i => new KendoDropdownListModel<string>()
                {
                    Value = i.ID.ToString(),
                    Text = $"{i.Name} ({i.Code})",
                    Datas = i
                }).ToList();

                return new Acknowledgement<List<KendoDropdownListModel<string>>>()
                {
                    IsSuccess = true,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new Acknowledgement<List<KendoDropdownListModel<string>>>()
                {
                    IsSuccess = false,
                    ErrorMessageList = [$"Error getting product dropdown list: {ex.Message}"]
                };
            }
        }

        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetUserDataDropdownList(string searchString, List<int> selectedIdList)
        {
            var predicate = PredicateBuilder.New<User>(i => i.IsActive == true);
            predicate = UserAuthorPredicate.GetUserAuthorPredicate(predicate, CurrentUserRoles, CurrentUserId);
            var selectedUserList = new List<User>();
            if (selectedIdList.Count > 0 && string.IsNullOrEmpty(searchString))
            {
                var tmpPredicate = PredicateBuilder.New<User>(predicate);
                tmpPredicate = tmpPredicate.And(i => selectedIdList.Contains(i.Id));
                selectedUserList = (await _userRepository.ReadOnlyRespository.GetAsync(tmpPredicate, i => i.OrderBy(p => p.Name))).ToList();
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringNonUnicode = Utils.NonUnicode(searchString.Trim().ToLower());
                predicate = predicate.And(i => (i.UserName.Trim().ToLower().Contains(searchStringNonUnicode) ||
                                                i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode))
                                         );
            }
            var userDbList = await _userRepository.ReadOnlyRespository.GetWithPagingAsync(new PagingParameters(1, 50 - selectedUserList.Count()), predicate, i => i.OrderBy(p => p.Name));
            var data = userDbList.Data.Concat(selectedUserList).Select(i => new KendoDropdownListModel<string>()
            {
                Value = i.Id.ToString(),
                Text = $"{i.Name} - {i.Code}",
            }).ToList();
            return new Acknowledgement<List<KendoDropdownListModel<string>>>()
            {
                IsSuccess = true,
                Data = data
            };
        }

        public async Task<Acknowledgement<List<KendoDropdownListModel<string>>>> GetDataOptionsDropdown(string? searchString, ECategoryType type)
        {
            var response = new Acknowledgement<List<KendoDropdownListModel<string>>>()
            {
                Data = [],
                IsSuccess = true
            };
            try
            {
                switch (type)
                {
                    case ECategoryType.CustomerType:
                        response.Data = GetCustomerType(searchString);
                        break;
                    case ECategoryType.Customer:
                        var customerAck = await GetCustomerDataDropdownList(searchString ?? "");
                        if (customerAck.IsSuccess)
                        {
                            response = customerAck;
                        }
                        else
                        {
                            response.ErrorMessageList = customerAck.ErrorMessageList;
                        }
                        break;
                    case ECategoryType.Unit:
                        var unitAck = await GetUnitDataDropdownList(searchString ?? "");
                        if (unitAck.IsSuccess)
                        {
                            response = unitAck;
                        }
                        else
                        {
                            response.ErrorMessageList = unitAck.ErrorMessageList;
                        }
                        break;
                    case ECategoryType.ProcessingType:
                        var processingTypeAck = await GetProcessingTypeDataDropdownList(searchString ?? "");
                        if (processingTypeAck.IsSuccess)
                        {
                            response = processingTypeAck;
                        }
                        else
                        {
                            response.ErrorMessageList = processingTypeAck.ErrorMessageList;
                        }
                        break;
                    case ECategoryType.Material:
                        var materialAck = await GetMaterialDataDropdownList(searchString ?? "");
                        if (materialAck.IsSuccess)
                        {
                            response = materialAck;
                        }
                        else
                        {
                            response.ErrorMessageList = materialAck.ErrorMessageList;
                        }
                        break;
                    case ECategoryType.Vendor:
                        var vendorAck = await GetVendorDataDropdownList(searchString ?? "");
                        if (vendorAck.IsSuccess)
                        {
                            response = vendorAck;
                        }
                        else
                        {
                            response.ErrorMessageList = vendorAck.ErrorMessageList;
                        }
                        break;
                    case ECategoryType.Product:
                        var productAck = await GetProductDataDropdownList(searchString ?? "");
                        if (productAck.IsSuccess)
                        {
                            response = productAck;
                        }
                        else
                        {
                            response.ErrorMessageList = productAck.ErrorMessageList;
                        }
                        break;
                    case ECategoryType.User:
                        var userAck = await GetUserDataDropdownList(searchString ?? "", new List<int>());
                        if (userAck.IsSuccess)
                        {
                            response = userAck;
                        }
                        else
                        {
                            response.ErrorMessageList = userAck.ErrorMessageList;
                        }
                        break;
                    default: break;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError("GetDataOptionsDropdown error: {ErrorMessage}", ex.Message);
                return response;
            }
        }
    }
}
