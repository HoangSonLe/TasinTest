using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class ReminderEntityConfigurations : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.UserId).HasColumnName("UserId");
            builder.Property(p => p.Content).HasColumnName("Content");
            builder.Property(p => p.RemindDate).HasColumnName("RemindDate");

            builder.Property(p => p.State).HasColumnName("State");
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");


            builder.HasOne(ur => ur.User).WithMany(r => r.Reminders).HasForeignKey(f => f.UserId);
            // Table
            builder.ToTable("Reminder");
        }
    }
}
