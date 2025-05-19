using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class UrnEntityConfigurations : IEntityTypeConfiguration<Urn>
    {
        public void Configure(EntityTypeBuilder<Urn> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.TenantId).HasColumnName("TenantId");
            builder.Property(p => p.Name).HasColumnName("Name");
            builder.Property(p => p.DharmaName).HasColumnName("DharmaName");
            builder.Property(p => p.BirthDate).HasColumnName("BirthDate");
            builder.Property(p => p.DeathDate).HasColumnName("DeathDate");
            builder.Property(p => p.Gender).HasColumnName("Gender");
            builder.Property(p => p.UrnType).HasColumnName("UrnType");
            builder.Property(p => p.Note).HasColumnName("Note");
            builder.Property(p => p.TowerLocation).HasColumnName("TowerLocation");
            builder.Property(p => p.CabinetName).HasColumnName("CabinetName");
            builder.Property(p => p.RowNumber).HasColumnName("RowNumber");
            builder.Property(p => p.BoxNumber).HasColumnName("BoxNumber");
            builder.Property(p => p.LocationNumber).HasColumnName("LocationNumber");
            builder.Property(p => p.NameNonUnicode).HasColumnName("NameNonUnicode");
            builder.Property(p => p.DharmaNameNonUnicode).HasColumnName("DharmaNameNonUnicode");
            builder.Property(p => p.TowerLocationNonUnicode).HasColumnName("TowerLocationNonUnicode");
            builder.Property(p => p.CabinetNameNonUnicode).HasColumnName("CabinetNameNonUnicode");

            builder.Property(p => p.State).HasColumnName("State");
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");
            // Table
            builder.ToTable("Urn");
        }
    }
}
