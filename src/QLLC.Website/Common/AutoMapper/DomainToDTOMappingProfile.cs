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
            CreateMap<Vendor, VendorViewModel>();
            CreateMap<VendorViewModel, Vendor>();
            CreateMap<Role, RoleViewModel>();

            // Unit mapping
            CreateMap<Unit, UnitViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ID));
            CreateMap<UnitViewModel, Unit>()
                .ForMember(dest => dest.ID, opts => opts.MapFrom(src => src.Id));

            // Category mapping
            CreateMap<Category, CategoryViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ID));
            CreateMap<CategoryViewModel, Category>()
                .ForMember(dest => dest.ID, opts => opts.MapFrom(src => src.Id));

            // ProcessingType mapping
            CreateMap<ProcessingType, ProcessingTypeViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ID));
            CreateMap<ProcessingTypeViewModel, ProcessingType>()
                .ForMember(dest => dest.ID, opts => opts.MapFrom(src => src.Id));

            // Material mapping
            CreateMap<Material, MaterialViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ID));
            CreateMap<MaterialViewModel, Material>()
                .ForMember(dest => dest.ID, opts => opts.MapFrom(src => src.Id));

            // SpecialProductTaxRate mapping
            CreateMap<SpecialProductTaxRate, SpecialProductTaxRateViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ID));
            CreateMap<SpecialProductTaxRateViewModel, SpecialProductTaxRate>()
                .ForMember(dest => dest.ID, opts => opts.MapFrom(src => src.Id));

            // Product mapping
            CreateMap<Product, ProductViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ID));
            CreateMap<ProductViewModel, Product>()
                .ForMember(dest => dest.ID, opts => opts.MapFrom(src => src.Id));
        }
    }
}
