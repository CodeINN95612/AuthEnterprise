using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthEnterprise.Api.Database.Configurations;

public class UserRoleConfigurations : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(r => new { r.UserId, r.RoleId });

        builder
            .Property(e => e.UserId)
            .HasColumnType("char(26)")
            .IsRequired()
            .HasConversion(
                p => p.ToString(),
                p => Ulid.Parse(p));

        builder
            .Property(e => e.RoleId)
            .HasColumnType("char(26)")
            .IsRequired()
            .HasConversion(
                p => p.ToString(),
                p => Ulid.Parse(p));

        builder
            .Property(e => e.AssinedAt)
            .IsRequired();
    }
}
