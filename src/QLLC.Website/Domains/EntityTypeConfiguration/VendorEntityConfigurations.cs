﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class VendorEntityConfigurations : IEntityTypeConfiguration<Vendor>
    {
        public void Configure(EntityTypeBuilder<Vendor> builder)
        {
            // Primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.Code).HasColumnName("Code").IsRequired().HasMaxLength(50);
            builder.Property(p => p.Name).HasColumnName("Name").IsRequired().HasMaxLength(255);
            builder.Property(p => p.NameNonUnicode).HasColumnName("NameNonUnicode").HasMaxLength(255);
            builder.Property(p => p.Address).HasColumnName("Address").HasColumnType("TEXT");
            builder.Property(p => p.Status).HasColumnName("Status").HasMaxLength(50);
            builder.Property(p => p.IsActive).HasColumnName("IsActived").HasDefaultValue(true);
            
            // Audit properties
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(100);
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(100);
            builder.Property(p => p.IsActive).HasColumnName("IsActived");

            // Indexes
            builder.HasIndex(p => p.Code).IsUnique();

            // Table
            builder.ToTable("Vendor");
        }
    }
}
