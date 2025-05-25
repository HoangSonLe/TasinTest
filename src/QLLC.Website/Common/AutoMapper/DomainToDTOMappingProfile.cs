using AutoMapper;
using Tasin.Website.Common.Enums;
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
                .ForMember(dest => dest.RoleIdList, opts => opts.MapFrom(i =>
                    string.IsNullOrWhiteSpace(i.RoleIdList) ? new List<int>() :
                    i.RoleIdList.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x.Trim())).ToList()))
                .ForMember(dest => dest.RoleViewList, opts => opts.Ignore())
                .ForMember(dest => dest.EnumActionList, opts => opts.Ignore())
                .ForMember(dest => dest.RoleName, opts => opts.Ignore())
                .ForMember(dest => dest.UpdatedByName, opts => opts.Ignore());
            CreateMap<UserViewModel, User>()
                .ForMember(dest => dest.RoleIdList, opts => opts.MapFrom(i =>
                    i.RoleIdList == null || i.RoleIdList.Count == 0 ? "" : string.Join(",", i.RoleIdList)))
                .ForMember(dest => dest.RoleList, opts => opts.Ignore());
            CreateMap<Customer, CustomerViewModel>();
            CreateMap<CustomerViewModel, Customer>();
            CreateMap<Vendor, VendorViewModel>();
            CreateMap<VendorViewModel, Vendor>();
            CreateMap<Role, RoleViewModel>();
            CreateMap<RoleViewModel, Role>();

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

            // PurchaseOrder mapping
            CreateMap<Purchase_Order, PurchaseOrderViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ID))
                //.ForMember(dest => dest.Status, opts => opts.MapFrom(src => ParsePOStatus(src.Status)))
                .ForMember(dest => dest.CustomerName, opts => opts.Ignore());
            CreateMap<PurchaseOrderViewModel, Purchase_Order>()
                .ForMember(dest => dest.ID, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => ((int)src.Status).ToString()))
                .ForMember(dest => dest.Customer, opts => opts.Ignore())
                .ForMember(dest => dest.PurchaseOrderItems, opts => opts.Ignore());

            // PurchaseOrderItem mapping
            CreateMap<Purchase_Order_Item, PurchaseOrderItemViewModel>()
                .ForMember(dest => dest.ProductName, opts => opts.Ignore())
                .ForMember(dest => dest.UnitName, opts => opts.Ignore())
                .ForMember(dest => dest.ProcessingTypeName, opts => opts.Ignore());
            CreateMap<PurchaseOrderItemViewModel, Purchase_Order_Item>()
                .ForMember(dest => dest.PurchaseOrder, opts => opts.Ignore())
                .ForMember(dest => dest.Product, opts => opts.Ignore())
                .ForMember(dest => dest.Unit, opts => opts.Ignore())
                .ForMember(dest => dest.ProcessingType, opts => opts.Ignore());

            // PurchaseAgreement mapping
            CreateMap<Purchase_Agreement, PurchaseAgreementViewModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ID))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => ParsePAStatus(src.Status)))
                .ForMember(dest => dest.VendorName, opts => opts.Ignore());
            CreateMap<PurchaseAgreementViewModel, Purchase_Agreement>()
                .ForMember(dest => dest.ID, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Vendor, opts => opts.Ignore())
                .ForMember(dest => dest.PurchaseAgreementItems, opts => opts.Ignore());

            // PurchaseAgreementItem mapping
            CreateMap<Purchase_Agreement_Item, PurchaseAgreementItemViewModel>()
                .ForMember(dest => dest.ProductName, opts => opts.Ignore())
                .ForMember(dest => dest.UnitName, opts => opts.Ignore());
            CreateMap<PurchaseAgreementItemViewModel, Purchase_Agreement_Item>()
                .ForMember(dest => dest.PurchaseAgreement, opts => opts.Ignore())
                .ForMember(dest => dest.Product, opts => opts.Ignore())
                .ForMember(dest => dest.Unit, opts => opts.Ignore());
        }

        private static EPAStatus ParsePAStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
                return EPAStatus.New;

            if (Enum.TryParse<EPAStatus>(status, out var result))
                return result;

            return EPAStatus.New;
        }

        //private static EPOStatus ParsePOStatus(string status)
        //{
        //    if (string.IsNullOrEmpty(status))
        //        return EPOStatus.New;

        //    if (int.TryParse(status, out var statusInt) && Enum.IsDefined(typeof(EPOStatus), statusInt))
        //        return (EPOStatus)statusInt;

        //    return EPOStatus.New;
        //}
    }
}
