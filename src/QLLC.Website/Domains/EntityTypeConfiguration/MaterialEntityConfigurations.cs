﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class MaterialEntityConfigurations : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            // Primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.Code).HasColumnName("Code").IsRequired().HasMaxLength(50);
            builder.Property(p => p.Name).HasColumnName("Name").IsRequired().HasMaxLength(255);
            builder.Property(p => p.NameNonUnicode).HasColumnName("NameNonUnicode").IsRequired().HasMaxLength(255);
            builder.Property(p => p.Name_EN).HasColumnName("Name_EN").HasMaxLength(255);
            builder.Property(p => p.Parent_ID).HasColumnName("Parent_ID");
            builder.Property(p => p.Description).HasColumnName("Description").HasColumnType("TEXT");
            builder.Property(p => p.Status).HasColumnName("Status").HasMaxLength(50);
            builder.Property(p => p.IsActive).HasColumnName("IsActive").HasDefaultValue(true);

            // Audit properties
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");

            // Relationships
            builder.HasOne(p => p.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(p => p.Parent_ID)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(p => p.Code).IsUnique();

            // Table
            builder.ToTable("Material");
        }
    }
}
