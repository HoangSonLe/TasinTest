using AutoMapper;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Models.ViewModels;
namespace Tasin.Website.Common.AutoMapper
{
    public class DomainToDTOMappingProfile : Profile
    {
        public DomainToDTOMappingProfile()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(dest => dest.UpdatedBy, opts => opts.MapFrom(i => i.UpdatedBy != null ? i.UpdatedBy : i.CreatedBy))
                .ForMember(dest => dest.UpdatedDate, opts => opts.MapFrom(i => i.UpdatedDate != null ? i.UpdatedDate : i.CreatedDate))
                .ForMember(dest => dest.RoleIdList, opts => opts.MapFrom(i => (i.RoleIdList ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(j => Int32.Parse(j)).ToList()));
            CreateMap<UserViewModel, User>();
            CreateMap<Customer, CustomerViewModel>();
            CreateMap<CustomerViewModel, Customer>();
            CreateMap<Role, RoleViewModel>();
        }
    }
}
