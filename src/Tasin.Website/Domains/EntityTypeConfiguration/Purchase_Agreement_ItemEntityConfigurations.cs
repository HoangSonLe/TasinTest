﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class Purchase_Agreement_ItemEntityConfigurations : IEntityTypeConfiguration<Purchase_Agreement_Item>
    {
        public void Configure(EntityTypeBuilder<Purchase_Agreement_Item> builder)
        {
            // Primary key
            builder.HasKey(p => new { p.PA_ID, p.Product_ID });

            // Properties
            builder.Property(p => p.PA_ID).HasColumnName("PA_ID").IsRequired();
            builder.Property(p => p.Product_ID).HasColumnName("Product_ID").IsRequired();
            builder.Property(p => p.Quantity).HasColumnName("Quantity").IsRequired().HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.Unit_ID).HasColumnName("Unit_ID");
            builder.Property(p => p.Price).HasColumnName("Price").HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.PO_Item_ID_List).HasColumnName("PO_Item_ID_List").HasColumnType("TEXT");

            // Relationships
            builder.HasOne(p => p.PurchaseAgreement)
                .WithMany(p => p.PurchaseAgreementItems)
                .HasForeignKey(p => p.PA_ID)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(p => p.Product)
                .WithMany(p => p.PurchaseAgreementItems)
                .HasForeignKey(p => p.Product_ID);
                
            builder.HasOne(p => p.Unit)
                .WithMany(p => p.PurchaseAgreementItems)
                .HasForeignKey(p => p.Unit_ID);

            // Table
            builder.ToTable("Purchase_Agreement_Item");
        }
    }
}
