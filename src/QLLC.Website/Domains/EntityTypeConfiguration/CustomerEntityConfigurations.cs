﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class CustomerEntityConfigurations : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.Code).HasColumnName("Code").IsRequired().HasMaxLength(50);
            builder.Property(p => p.Name).HasColumnName("Name").IsRequired().HasMaxLength(255);
            builder.Property(p => p.NameNonUnicode).HasColumnName("NameNonUnicode").HasMaxLength(255);
            builder.Property(p => p.Type).HasColumnName("Type").HasMaxLength(50);
            builder.Property(p => p.PhoneContact).HasColumnName("PhoneContact").HasMaxLength(50);
            builder.Property(p => p.Email).HasColumnName("Email").HasMaxLength(255);
            builder.Property(p => p.TaxCode).HasColumnName("TaxCode").HasMaxLength(100);
            builder.Property(p => p.Address).HasColumnName("Address").HasColumnType("TEXT");
            builder.Property(p => p.IsActived).HasColumnName("IsActived").HasDefaultValue(true);

            // Audit properties
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(100);
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(100);

            // Indexes
            builder.HasIndex(p => p.Code).IsUnique();

            // Table
            builder.ToTable("Customer");
        }
    }
}
