using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class RoleEntityConfigurations : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Name).HasColumnName("Name");
            builder.Property(p => p.Description).HasColumnName("Description");
            builder.Property(p => p.EnumActionList).HasColumnName("EnumActionList");
            builder.Property(p => p.Level).HasColumnName("Level");

            // Table
            builder.ToTable("Role");
        }
    }
}
