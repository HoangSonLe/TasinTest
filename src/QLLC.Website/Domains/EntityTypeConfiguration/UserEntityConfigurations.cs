using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class UserEntityConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Primary key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.Code).HasColumnName("Code");
            builder.Property(p => p.UserName).HasColumnName("UserName");
            builder.Property(p => p.Password).HasColumnName("Password");
            builder.Property(p => p.Name).HasColumnName("Name");
            builder.Property(p => p.Email).HasColumnName("Email");
            builder.Property(p => p.Address).HasColumnName("Address");
            builder.Property(p => p.Phone).HasColumnName("Phone");
            builder.Property(p => p.TypeAccount).HasColumnName("TypeAccount");
            builder.Property(p => p.NameNonUnicode).HasColumnName("NameNonUnicode");
            builder.Property(p => p.RoleIdList).HasColumnName("RoleIdList");
            builder.Property(p => p.IsActived).HasColumnName("IsActived").HasDefaultValue(true);

            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy");
            builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(p => p.UpdatedBy).HasColumnName("UpdatedBy");

            // Table
            builder.ToTable("User");
        }
    }
}
