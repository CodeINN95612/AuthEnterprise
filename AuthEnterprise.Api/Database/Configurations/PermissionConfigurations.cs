using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthEnterprise.Api.Database.Configurations;

public class PermissionConfigurations : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(r => r.Id);

        builder
            .Property(e => e.Id)
            .HasColumnType("char(26)")
            .IsRequired()
            .HasConversion(
                p => p.ToString(),
                p => Ulid.Parse(p));

        builder
            .Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder
            .Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder
            .Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(300);

        builder
            .Property(r => r.Module)
            .IsRequired()
            .HasMaxLength(20);

        builder
            .Property(r => r.Action)
            .IsRequired()
            .HasMaxLength(20);
    }
}
