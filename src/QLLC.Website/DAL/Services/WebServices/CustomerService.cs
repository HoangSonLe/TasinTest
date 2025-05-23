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
using Tasin.Website.Models.ViewModels.AccountViewModels;


namespace Tasin.Website.DAL.Services.WebServices
{
    public class CustomerService : BaseService<CustomerService>, ICustomerService
    {
        private readonly IMapper _mapper;
        private ICustomerRepository _customerRepository;
        public CustomerService(
            ILogger<CustomerService> logger,
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IRoleRepository roleRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ICurrentUserContext currentUserContext,
            SampleDBContext dbContext,
            IMapper mapper
            ) : base(logger, configuration, userRepository, roleRepository, httpContextAccessor, currentUserContext, dbContext)
        {
            _mapper = mapper;
            _customerRepository = customerRepository;
        }


        public async Task<Acknowledgement<List<KendoDropdownListModel<int>>>> GetCustomerDataDropdownList(string searchString)
        {
            var predicate = PredicateBuilder.New<Customer>(i => i.IsActive == true);
            predicate = CustomerAuthorPredicate.GetCustomerAuthorPredicate(predicate, _currentUserRoleId, _currentUserId);
            var selectedUserList = new List<User>();
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchStringNonUnicode = Utils.NonUnicode(searchString.Trim().ToLower());
                predicate = predicate.And(i => i.NameNonUnicode.Trim().ToLower().Contains(searchStringNonUnicode));
            }
            var customerDBList = await _customerRepository.ReadOnlyRespository.GetWithPagingAsync(new PagingParameters(1, 50), predicate, i => i.OrderBy(p => p.Name));
            var data = customerDBList.Data.Select(i => new KendoDropdownListModel<int>()
            {
                Value = i.ID.ToString(),
                Text = $"{i.Name} - {i.PhoneContact}",
            }).ToList();
            return new Acknowledgement<List<KendoDropdownListModel<int>>>()
            {
                IsSuccess = true,
                Data = data
            };
        }

        public async Task<Acknowledgement<JsonResultPaging<List<CustomerViewModel>>>> GetCustomerList(CustomerSearchModel searchModel)
        {
            var response = new Acknowledgement<JsonResultPaging<List<CustomerViewModel>>>();
            try
            {
                var predicate = PredicateBuilder.New<Customer>(i => i.IsActive == true);

                if (!string.IsNullOrEmpty(searchModel.SearchString))
                {
                    var searchStringNonUnicode = Utils.NonUnicode(searchModel.SearchString.Trim().ToLower());
                    predicate = predicate.And(i => i.NameNonUnicode.ToLower().Contains(searchStringNonUnicode) ||
                                                    (string.IsNullOrEmpty(i.PhoneContact) == false && i.PhoneContact.Trim().ToLower().Contains(searchStringNonUnicode))
                                             );
                }

                var userList = new List<CustomerViewModel>();
                predicate = CustomerAuthorPredicate.GetCustomerAuthorPredicate(predicate, _currentUserRoleId, _currentUserId);
                var customerQuery = await _customerRepository.ReadOnlyRespository.GetWithPagingAsync(
                    new PagingParameters(searchModel.PageNumber, searchModel.PageSize),
                    predicate,
                    i => i.OrderByDescending(u => u.UpdatedDate)
                    );
                var customerDBList = _mapper.Map<List<CustomerViewModel>>(customerQuery.Data);
                var updateByUserIdList = customerDBList.Select(i => i.UpdatedBy).ToList();
                var updateByUserList = await _userRepository.ReadOnlyRespository.GetAsync(i => updateByUserIdList.Contains(i.Id));
                foreach (var user in customerDBList)
                {
                    var updateUser = updateByUserList.FirstOrDefault(i => i.Id == user.UpdatedBy);
                    user.UpdatedByName = updateUser.Name;
                }
                response.Data = new JsonResultPaging<List<CustomerViewModel>>()
                {
                    Data = customerDBList,
                    PageNumber = searchModel.PageNumber,
                    PageSize = searchModel.PageSize,
                    Total = customerQuery.TotalRecords
                };
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                response.ExtractMessage(ex);
                _logger.LogError("GetCustomerList " + ex.Message);
                return response;

            }
        }

        public async Task<Acknowledgement<CustomerViewModel>> GetCustomerById(int userId)
        {
            var ack = new Acknowledgement<CustomerViewModel>();
            try
            {
                var customer = await _customerRepository.ReadOnlyRespository.FindAsync(userId);
                if (customer == null)
                {
                    ack.IsSuccess = false;
                    ack.AddMessages("Không tìm thấy khách hàng");
                    return ack;
                }

                ack.Data = _mapper.Map<CustomerViewModel>(customer);
                ack.IsSuccess = true;
                return ack;

            }
            catch (Exception ex)
            {
                ack.ExtractMessage(ex);
                _logger.LogError("GetCustomerById " + ex.Message);
                return ack;
            }

        }

        public async Task<Acknowledgement> CreateOrUpdateCustomer(CustomerViewModel postData)
        {
            var ack = new Acknowledgement();
            if (string.IsNullOrWhiteSpace(postData.Name))
            {
                ack.AddMessage("Vui lòng nhập họ tên");
                return ack;
            }
            if (postData.Type == ECustomerType.Company && string.IsNullOrWhiteSpace(postData.TaxCode))
            {
                ack.AddMessage("Vui lòng nhập mã số thuế");
                return ack;
            }
            var phone = postData.PhoneContact;
            var validatePhoneMessage = Validate.ValidPhoneNumber(ref phone);
            if (validatePhoneMessage != null)
            {
                ack.AddMessage(validatePhoneMessage);
                return ack;
            }
            postData.PhoneContact = phone;
            if (!string.IsNullOrWhiteSpace(postData.Email))
            {
                var isValidEmail = Validate.ValidEmail(postData.Email);
                if (!isValidEmail)
                {
                    ack.AddMessage("Email không đúng định dạng.");
                    return ack;
                }
            }

            if (postData.Id == 0)
            {
                var newCustomer = _mapper.Map<Customer>(postData);
                newCustomer.Code = await Generator.GenerateEntityCodeAsync(EntityPrefix.Customer, DbContext);
                newCustomer.NameNonUnicode = Utils.NonUnicode(newCustomer.Name);
                newCustomer.CreatedDate = DateTime.Now;
                newCustomer.CreatedBy = _currentUserId;
                newCustomer.UpdatedDate = newCustomer.CreatedDate;
                newCustomer.UpdatedBy = newCustomer.CreatedBy;
                await ack.TrySaveChangesAsync(res => res.AddAsync(newCustomer), _customerRepository.Repository);
            }
            else
            {
                var existItem = await _customerRepository.Repository.FirstOrDefaultAsync(i => i.ID == postData.Id && i.IsActive == true);
                if (existItem == null)
                {
                    ack.AddMessage("Không tìm thấy khách hàng");
                    ack.IsSuccess = false;
                    return ack;
                }
                else
                {
                    existItem.Name = postData.Name;
                    existItem.NameNonUnicode = Utils.NonUnicode(postData.Name);
                    existItem.PhoneContact = postData.PhoneContact;
                    existItem.Email = postData.Email;
                    existItem.TaxCode = postData.TaxCode;
                    existItem.Address = postData.Address;
                    existItem.UpdatedDate = DateTime.Now;
                    existItem.UpdatedBy = _currentUserId;
                    await ack.TrySaveChangesAsync(res => res.UpdateAsync(existItem), _customerRepository.Repository);
                }
            }
            return ack;
        }

        public async Task<Acknowledgement> DeleteCustomerById(int userId)
        {
            var ack = new Acknowledgement();
            var customer = await _customerRepository.Repository.FirstOrDefaultAsync(i => i.ID == userId);
            if (customer == null)
            {
                ack.AddMessage("Không tìm thấy khách hàng");
                return ack;
            }
            customer.IsActive = false;
            await ack.TrySaveChangesAsync(res => res.UpdateAsync(customer), _customerRepository.Repository);
            return ack;
        }
    }
}
