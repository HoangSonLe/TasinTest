﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class Purchase_Order_ItemEntityConfigurations : IEntityTypeConfiguration<Purchase_Order_Item>
    {
        public void Configure(EntityTypeBuilder<Purchase_Order_Item> builder)
        {
            // Primary key - Use ID as single primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.PO_ID).HasColumnName("PO_ID").IsRequired();
            builder.Property(p => p.ID).HasColumnName("ID").IsRequired().ValueGeneratedOnAdd();
            builder.Property(p => p.Product_ID).HasColumnName("Product_ID").IsRequired();
            builder.Property(p => p.Quantity).HasColumnName("Quantity").IsRequired().HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.Unit_ID).HasColumnName("Unit_ID");
            builder.Property(p => p.Price).HasColumnName("Price").HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.TaxRate).HasColumnName("TaxRate").HasColumnType("NUMERIC(5, 2)");
            builder.Property(p => p.LossRate).HasColumnName("LossRate").HasColumnType("NUMERIC(5, 2)");
            builder.Property(p => p.AdditionalCost).HasColumnName("AdditionalCost").HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.ProcessingFee).HasColumnName("ProcessingFee").HasColumnType("NUMERIC(18, 2)");
            builder.Property(p => p.Note).HasColumnName("Note").HasColumnType("TEXT");

            // Relationships
            builder.HasOne(p => p.PurchaseOrder)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(p => p.PO_ID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Product)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(p => p.Product_ID);

            builder.HasOne(p => p.Unit)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(p => p.Unit_ID);

            // Indexes
            builder.HasIndex(p => new { p.PO_ID, p.Product_ID })
                .HasDatabaseName("IX_Purchase_Order_Item_PO_Product")
                .IsUnique();

            // Table
            builder.ToTable("Purchase_Order_Item");
        }
    }
}
