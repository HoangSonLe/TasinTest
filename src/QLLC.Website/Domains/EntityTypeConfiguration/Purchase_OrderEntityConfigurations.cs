﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class Purchase_OrderEntityConfigurations : IEntityTypeConfiguration<Purchase_Order>
    {
        public void Configure(EntityTypeBuilder<Purchase_Order> builder)
        {
            // Primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.Customer_ID).HasColumnName("Customer_ID").IsRequired();
            builder.Property(p => p.TotalPrice).HasColumnName("TotalPrice").IsRequired().HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.TotalPriceNoTax).HasColumnName("TotalPriceNoTax").IsRequired().HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.Code).HasColumnName("Code").IsRequired().HasMaxLength(50);
            builder.Property(p => p.Status).HasColumnName("Status").HasMaxLength(50);
            builder.Property(p => p.IsActived).HasColumnName("IsActived").HasDefaultValue(true);
            
            // Audit properties
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(100);
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(100);

            // Relationships
            builder.HasOne(p => p.Customer)
                .WithMany(p => p.PurchaseOrders)
                .HasForeignKey(p => p.Customer_ID);

            // Indexes
            builder.HasIndex(p => p.Code).IsUnique();

            // Table
            builder.ToTable("Purchase_Order");
        }
    }
}
