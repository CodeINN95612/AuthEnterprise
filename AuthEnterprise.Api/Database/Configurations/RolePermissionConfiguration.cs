using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthEnterprise.Api.Database.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(r => new { r.RoleId, r.PermissionId });

        builder
            .Property(e => e.RoleId)
            .HasColumnType("char(26)")
            .IsRequired()
            .HasConversion(
                p => p.ToString(),
                p => Ulid.Parse(p));

        builder
            .Property(e => e.PermissionId)
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
