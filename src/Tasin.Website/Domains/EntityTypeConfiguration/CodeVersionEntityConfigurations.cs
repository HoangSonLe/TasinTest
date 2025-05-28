﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class CodeVersionEntityConfigurations : IEntityTypeConfiguration<CodeVersion>
    {
        public void Configure(EntityTypeBuilder<CodeVersion> builder)
        {
            // Primary key
            builder.HasKey(p => p.ID);

            // Properties
            builder.Property(p => p.Prefix).HasColumnName("Prefix").IsRequired().HasMaxLength(50);
            builder.Property(p => p.VersionIndex).HasColumnName("VersionIndex").IsRequired();
            builder.Property(p => p.Type).HasColumnName("Type").HasMaxLength(50);
            builder.Property(p => p.Note).HasColumnName("Note").HasColumnType("TEXT");

            // Table
            builder.ToTable("CodeVersion");
        }
    }
}
