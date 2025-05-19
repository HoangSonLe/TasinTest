using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class ProvincesEntityConfigurations : IEntityTypeConfiguration<Provinces>
    {
        public void Configure(EntityTypeBuilder<Provinces> builder)
        {
            // Primary key
            builder.HasKey(p => p.id);

            // Properties
            builder.Property(p => p.name).HasColumnName("name");
            builder.Property(p => p.code).HasColumnName("code");
            builder.Property(p => p.name_en).HasColumnName("name_en");
           

            // Table
            builder.ToTable("Provinces");
        }
    }
}
