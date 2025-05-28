﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class Purchase_AgreementEntityConfigurations : IEntityTypeConfiguration<Purchase_Agreement>
    {
        public void Configure(EntityTypeBuilder<Purchase_Agreement> builder)
        {
            // Primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.Vendor_ID).HasColumnName("Vendor_ID").IsRequired();
            builder.Property(p => p.Note).HasColumnName("Note").HasColumnType("TEXT");
            builder.Property(p => p.Code).HasColumnName("Code").IsRequired().HasMaxLength(50);
            builder.Property(p => p.GroupCode).HasColumnName("GroupCode").IsRequired().HasMaxLength(50);
            builder.Property(p => p.TotalPrice).HasColumnName("TotalPrice").IsRequired().HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.Status).HasColumnName("Status").HasMaxLength(50);
            builder.Property(p => p.IsActive).HasColumnName("IsActive").HasDefaultValue(true);

            // Audit properties
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(100);
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(100);

            // Relationships
            builder.HasOne(p => p.Vendor)
                .WithMany(p => p.PurchaseAgreements)
                .HasForeignKey(p => p.Vendor_ID)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(p => p.Code).IsUnique();
            builder.HasIndex(p => p.GroupCode);

            // Table
            builder.ToTable("Purchase_Agreement");
        }
    }
}
