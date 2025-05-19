using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasin.Website.Domains.Entitites;

namespace Tasin.Website.Domains.EntityTypeConfiguration
{
    public class User_UrnEntityConfigurations : IEntityTypeConfiguration<User_Urn>
    {
        public void Configure(EntityTypeBuilder<User_Urn> builder)
        {
            // Properties
            builder.Property(p => p.UserId).HasColumnName("UserId");
            builder.Property(p => p.UrnId).HasColumnName("UrnId");

            // Table
            builder.ToTable("User_Urn");
        }
    }
}
