﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class TaxRateConfigEntityConfigurations : IEntityTypeConfiguration<TaxRateConfig>
    {
        public void Configure(EntityTypeBuilder<TaxRateConfig> builder)
        {
            // Primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.CompanyTaxRate).HasColumnName("CompanyTaxRate").IsRequired().HasColumnType("NUMERIC(5, 2)");
            builder.Property(p => p.ConsumerTaxRate).HasColumnName("ConsumerTaxRate").IsRequired().HasColumnType("NUMERIC(5, 2)");
            builder.Property(p => p.SpecialProductTaxRate_ID).HasColumnName("SpecialProductTaxRate_ID");
            builder.Property(p => p.Status).HasColumnName("Status").HasMaxLength(50);
            builder.Property(p => p.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
            
            // Audit properties
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(100);
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(100);

            // Relationships
            builder.HasOne(p => p.SpecialProductTaxRate)
                .WithMany(p => p.TaxRateConfigs)
                .HasForeignKey(p => p.SpecialProductTaxRate_ID)
                .OnDelete(DeleteBehavior.SetNull);

            // Table
            builder.ToTable("TaxRateConfig");
        }
    }
}
