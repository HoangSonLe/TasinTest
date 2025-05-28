﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class Product_VendorEntityConfigurations : IEntityTypeConfiguration<Product_Vendor>
    {
        public void Configure(EntityTypeBuilder<Product_Vendor> builder)
        {
            // Primary key
            builder.HasKey(p => new { p.Vendor_ID, p.Product_ID });

            // Properties
            builder.Property(p => p.Vendor_ID).HasColumnName("Vendor_ID").IsRequired();
            builder.Property(p => p.Product_ID).HasColumnName("Product_ID").IsRequired();
            builder.Property(p => p.Price).HasColumnName("Price").HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.UnitPrice).HasColumnName("UnitPrice").HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.Priority).HasColumnName("Priority");
            builder.Property(p => p.Description).HasColumnName("Description").HasColumnType("TEXT");

            // Relationships
            builder.HasOne(p => p.Vendor)
                .WithMany(p => p.ProductVendors)
                .HasForeignKey(p => p.Vendor_ID);
                
            builder.HasOne(p => p.Product)
                .WithMany(p => p.ProductVendors)
                .HasForeignKey(p => p.Product_ID);

            // Table
            builder.ToTable("Product_Vendor");
        }
    }
}
