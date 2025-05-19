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
                .ForMember(dest => dest.TenantName, opts => opts.MapFrom(i => i.Tenant != null ? i.Tenant.Name : ""))
                .ForMember(dest => dest.UpdatedBy, opts => opts.MapFrom(i => i.UpdatedBy != null ? i.UpdatedBy : i.CreatedBy))
                .ForMember(dest => dest.UpdatedDate, opts => opts.MapFrom(i => i.UpdatedDate != null ? i.UpdatedDate : i.CreatedDate));
                //.ForMember(dest => dest.RoleName, opts => opts.MapFrom(i => string.Join(",", i.RoleList.Select(j => j.Name))));
            CreateMap<UserViewModel, User>();
            CreateMap<Urn, UrnViewModel>()
                .ForMember(dest => dest.FamilyMemberList, opts => opts.MapFrom(i => i.FamilyMembers == null ? new List<UserViewModel>() : i.FamilyMembers.Select(i => new UserViewModel()
                {
                    Id = i.UserId,
                    Name = i.User == null ? "" : i.User.Name,
                    Phone = i.User == null ? "" : i.User.Phone,
                    Address = i.User == null ? "" : i.User.Address
                }).ToList()))
                .ForMember(dest => dest.IsHasImage, opts => opts.MapFrom(i => string.IsNullOrWhiteSpace(i.FileImageUrl) ? false : true))
                .ForMember(dest => dest.FamilyMemberIdList, opts => opts.MapFrom(i => i.FamilyMembers == null ? new List<int>() : i.FamilyMembers.Select(i => i.UserId).ToList()));
            CreateMap<UrnViewModel, Urn>();
            CreateMap<Role, RoleViewModel>();
            CreateMap<RoleViewModel, Role>();
            CreateMap<TenantViewModel, Tenant>();
            CreateMap<Tenant, TenantViewModel>();
            CreateMap<Reminder, ReminderViewModel>();
            CreateMap<ReminderViewModel, Reminder>();
            CreateMap<ConfigViewModel, Config>();
            CreateMap<Config, ConfigViewModel>();


            CreateMap<StorageMap, StorageMapViewModel>();
            CreateMap<StorageMapViewModel, StorageMap>();
            CreateMap<HistoryLoginViewModel, TelegramChat>();
            CreateMap<TelegramChat, HistoryLoginViewModel>();
        }
    }
}
