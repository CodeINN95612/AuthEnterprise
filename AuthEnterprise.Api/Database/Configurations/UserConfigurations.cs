using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthEnterprise.Api.Database.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder
            .Property(e => e.Id)
            .HasColumnType("char(26)")
            .IsRequired()
            .HasConversion(
                p => p.ToString(),
                p => Ulid.Parse(p));

        builder
            .Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder
            .Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder
            .Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder
            .Property(e => e.CreatedAt)
            .IsRequired();

        builder
            .Property(e => e.UpdatedAt)
            .IsRequired();

        builder
            .Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder
            .HasIndex(p => p.Username)
            .IsUnique();

        builder
            .HasIndex(p => p.Email)
            .IsUnique();
    }
}
