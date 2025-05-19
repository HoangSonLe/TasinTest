using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class ConfigEntityConfigurations : IEntityTypeConfiguration<Config>
    {
        public void Configure(EntityTypeBuilder<Config> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.TenantId).HasColumnName("TenantId");
            builder.Property(p => p.TenantName).HasColumnName("TenantName");
            builder.Property(p => p.NumberOfDaysNoticeAnniversary).HasColumnName("NumberOfDaysNoticeAnniversary");
            builder.Property(p => p.NumberOfDaysNoticeExpiredUrn).HasColumnName("NumberOfDaysNoticeExpiredUrn");
            builder.Property(p => p.RemindNotification).HasColumnName("RemindNotification");
            builder.Property(p => p.MonthGeneralNotification).HasColumnName("MonthGeneralNotification");
            builder.Property(p => p.DayGeneralNotification).HasColumnName("DayGeneralNotification");
            builder.Property(p => p.State).HasColumnName("State");
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");
            
            // Table
            builder.ToTable("Config");
        }
    }
}
