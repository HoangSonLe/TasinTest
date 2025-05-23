using Microsoft.EntityFrameworkCore;
using Tasin.Website.Common.Enums;
using Tasin.Website.Domains.Entitites;
using Tasin.Website.Domains.EntityTypeConfiguration;

namespace Tasin.Website.Domains.DBContexts
{
    public class SampleReadOnlyDBContext : SampleDBContext
    {
        public SampleReadOnlyDBContext() : base()
        {
        }
        public SampleReadOnlyDBContext(DbContextOptions<SampleDBContext> options)
           : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        public override int SaveChanges()
        {
            throw new NotSupportedException();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
    public class SampleDBContext : DbContext
    {
        public SampleDBContext(DbContextOptions<SampleDBContext> options)
           : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        public SampleDBContext() : base()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }

        // New entities from InitDatabase.sql
        public virtual DbSet<Vendor> Vendors { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<ProcessingType> ProcessingTypes { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<SpecialProductTaxRate> SpecialProductTaxRates { get; set; }
        public virtual DbSet<TaxRateConfig> TaxRateConfigs { get; set; }
        public virtual DbSet<Material> Materials { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Product_Vendor> ProductVendors { get; set; }
        public virtual DbSet<Purchase_Order> PurchaseOrders { get; set; }
        public virtual DbSet<Purchase_Order_Item> PurchaseOrderItems { get; set; }
        public virtual DbSet<Purchase_Agreement> PurchaseAgreements { get; set; }
        public virtual DbSet<Purchase_Agreement_Item> PurchaseAgreementItems { get; set; }
        public virtual DbSet<CodeVersion> CodeVersions { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            modelBuilder.ApplyConfiguration(new UserEntityConfigurations());
            modelBuilder.ApplyConfiguration(new RoleEntityConfigurations());

            // Apply configurations for new entities
            modelBuilder.ApplyConfiguration(new VendorEntityConfigurations());
            modelBuilder.ApplyConfiguration(new UnitEntityConfigurations());
            modelBuilder.ApplyConfiguration(new CategoryEntityConfigurations());
            modelBuilder.ApplyConfiguration(new ProcessingTypeEntityConfigurations());
            modelBuilder.ApplyConfiguration(new CustomerEntityConfigurations());
            modelBuilder.ApplyConfiguration(new SpecialProductTaxRateEntityConfigurations());
            modelBuilder.ApplyConfiguration(new TaxRateConfigEntityConfigurations());
            modelBuilder.ApplyConfiguration(new MaterialEntityConfigurations());
            modelBuilder.ApplyConfiguration(new ProductEntityConfigurations());
            modelBuilder.ApplyConfiguration(new Product_VendorEntityConfigurations());
            modelBuilder.ApplyConfiguration(new Purchase_OrderEntityConfigurations());
            modelBuilder.ApplyConfiguration(new Purchase_Order_ItemEntityConfigurations());
            modelBuilder.ApplyConfiguration(new Purchase_AgreementEntityConfigurations());
            modelBuilder.ApplyConfiguration(new Purchase_Agreement_ItemEntityConfigurations());
            modelBuilder.ApplyConfiguration(new CodeVersionEntityConfigurations());

            //Default value for column entity models

            //modelBuilder.Entity<Role>().HasData(
            //    new Role()
            //    {
            //        Id = 1,
            //        Name = "Admin",
            //        Description = "Admin",
            //        EnumActionList = "1,2",
            //        NameNonUnicode = "Admin",
            //    },
            //    new Role()
            //    {
            //        Id = 2,
            //        Name = "Dev",
            //        Description = "Dev",
            //        EnumActionList = "1,2",
            //        NameNonUnicode = "Dev"
            //    }
            //);
            //modelBuilder.Entity<User>().HasData(
            //    new User()
            //    {
            //        Id = 1,
            //        UserName = "Admin",
            //        NameNonUnicode = "Admin",
            //        Password = "/cA7ZZQqtyOGVwe1kEbPSg==", //123456
            //        Name = "Admin",
            //        Phone = "",
            //        TypeAccount = 1,
            //        RoleIdList = "1,2,3,4",
            //        CreatedBy = 1,
            //        CreatedDate = DateTime.Now,
            //        IsActive = true,
            //        Email = ""
            //    }
            //);



            base.OnModelCreating(modelBuilder);
        }

    }
}
