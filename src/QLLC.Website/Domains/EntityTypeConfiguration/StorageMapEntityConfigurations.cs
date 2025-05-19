using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class StorageMapEntityConfigurations : IEntityTypeConfiguration<StorageMap>
    {
        public void Configure(EntityTypeBuilder<StorageMap> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.TenantId).HasColumnName("TenantId");
            builder.Property(p => p.Image).HasColumnName("Image");
            builder.Property(p => p.Location).HasColumnName("Location");
            builder.Property(p => p.Description).HasColumnName("Description");
            builder.Property(p => p.LocationNonUnicode).HasColumnName("LocationNonUnicode");

            builder.Property(p => p.State).HasColumnName("State");
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");

            // Table
            builder.ToTable("StorageMap");
        }
    }
}
