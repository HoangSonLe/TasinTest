﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class ProductEntityConfigurations : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.Code).HasColumnName("Code").IsRequired().HasMaxLength(50);
            builder.Property(p => p.Name).HasColumnName("Name").IsRequired().HasMaxLength(255);
            builder.Property(p => p.NameNonUnicode).HasColumnName("NameNonUnicode").HasMaxLength(255);
            builder.Property(p => p.Name_EN).HasColumnName("Name_EN").HasMaxLength(255);
            builder.Property(p => p.Unit_ID).HasColumnName("Unit_ID");
            builder.Property(p => p.Category_ID).HasColumnName("Category_ID");
            builder.Property(p => p.ProcessingType_ID).HasColumnName("ProcessingType_ID");
            builder.Property(p => p.TaxRate).HasColumnName("TaxRate").HasColumnType("NUMERIC(5, 2)");
            builder.Property(p => p.TaxRateConfig_ID).HasColumnName("TaxRateConfig_ID");
            builder.Property(p => p.LossRate).HasColumnName("LossRate").HasColumnType("NUMERIC(5, 2)");
            builder.Property(p => p.Material_ID).HasColumnName("Material_ID");
            builder.Property(p => p.ProfitMargin).HasColumnName("ProfitMargin").HasColumnType("NUMERIC(5, 2)");
            builder.Property(p => p.Note).HasColumnName("Note").HasColumnType("TEXT");
            builder.Property(p => p.IsDiscontinued).HasColumnName("IsDiscontinued").HasDefaultValue(false);
            builder.Property(p => p.ProcessingFee).HasColumnName("ProcessingFee").HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.Status).HasColumnName("Status").HasMaxLength(50);
            builder.Property(p => p.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
            
            // Audit properties
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(100);
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(100);

            // Relationships
            builder.HasOne(p => p.Unit)
                .WithMany(p => p.Products)
                .HasForeignKey(p => p.Unit_ID);
                
            builder.HasOne(p => p.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(p => p.Category_ID);
                
            builder.HasOne(p => p.ProcessingType)
                .WithMany(p => p.Products)
                .HasForeignKey(p => p.ProcessingType_ID);
                
            builder.HasOne(p => p.TaxRateConfig)
                .WithMany(p => p.Products)
                .HasForeignKey(p => p.TaxRateConfig_ID);
                
            builder.HasOne(p => p.Material)
                .WithMany(p => p.Products)
                .HasForeignKey(p => p.Material_ID);

            // Indexes
            builder.HasIndex(p => p.Code).IsUnique();

            // Table
            builder.ToTable("Product");
        }
    }
}
