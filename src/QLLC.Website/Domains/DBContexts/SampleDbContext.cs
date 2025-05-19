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
        public virtual DbSet<Reminder> Reminders { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<StorageMap> StorageMaps { get; set; }
        public virtual DbSet<Tenant> Tenants { get; set; }
        public virtual DbSet<Urn> Urns { get; set; }
        public virtual DbSet<User_Urn> User_Urns { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Provinces> Provincess { get; set; }
        public virtual DbSet<TelegramChat> TelegramChats { get; set; }
        public virtual DbSet<Config> Configs { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User_Urn>().HasKey(sc => new { sc.UserId, sc.UrnId });

            modelBuilder.Entity<User_Urn>()
                .HasOne<User>(sc => sc.User)
                .WithMany(s => s.UrnList)
                .HasForeignKey(sc => sc.UserId);


            modelBuilder.Entity<User_Urn>()
                .HasOne<Urn>(sc => sc.Urn)
                .WithMany(s => s.FamilyMembers)
                .HasForeignKey(sc => sc.UrnId);


            modelBuilder.ApplyConfiguration(new UserEntityConfigurations());
            modelBuilder.ApplyConfiguration(new ReminderEntityConfigurations());
            modelBuilder.ApplyConfiguration(new RoleEntityConfigurations());
            modelBuilder.ApplyConfiguration(new StorageMapEntityConfigurations());
            modelBuilder.ApplyConfiguration(new TenantEntityConfigurations());
            modelBuilder.ApplyConfiguration(new ProvincesEntityConfigurations());
            modelBuilder.ApplyConfiguration(new TelegramChatEntityConfigurations());
            modelBuilder.ApplyConfiguration(new ConfigEntityConfigurations());

            //Default value for column entity models

            modelBuilder.Entity<Role>().HasData(
                new Role()
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Admin",
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now,
                    State = (int)EState.Active,
                    EnumActionList = new List<int> { 1, 2 },
                    NameNonUnicode = "Admin",
                },
                new Role()
                {
                    Id = 2,
                    Name = "Dev",
                    Description = "Dev",
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now,
                    State = (int)EState.Active,
                    EnumActionList = new List<int> { 1, 2 },
                    NameNonUnicode = "Dev"
                }
            );
            modelBuilder.Entity<Tenant>().HasData(
               new Tenant()
               {
                   Id = 1,
                   Name = "Admin",
                   NameNonUnicode = "Admin",
                   Address = "Địa chỉ chùa",
                   AddressNonUnicode = "Đia chi chua",
                   CreatedBy = 1,
                   CreatedDate = DateTime.Now,
                   State = (int)EState.Active,
                   Code="TEST"
               }
           );
            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    Id = 1,
                    TenantId = 1,//tk admin
                    UserName = "Admin",
                    NameNonUnicode = "Admin",
                    Password = "/cA7ZZQqtyOGVwe1kEbPSg==", //123456
                    Name = "Admin",
                    Phone = "",
                    TypeAccount = 1,
                    RoleIdList = new List<int>() { 1,2,3,4},
                    CreatedBy = 1,
                    CreatedDate = DateTime.Now,
                    State = (int)EState.Active,
                    Email = ""
                }
            );



            base.OnModelCreating(modelBuilder);
        }

    }
}
