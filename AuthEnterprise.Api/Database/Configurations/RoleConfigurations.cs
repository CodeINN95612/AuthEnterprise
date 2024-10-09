using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthEnterprise.Api.Database.Configurations;

public class RoleConfigurations : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder
            .Property(e => e.Id)
            .HasColumnType("char(26)")
            .IsRequired()
            .HasConversion(
                p => p.ToString(),
                p => Ulid.Parse(p));

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(300);

        builder
            .Property(e => e.CreatedAt)
            .IsRequired();
    }
}