using Microsoft.EntityFrameworkCore;

namespace AuthEnterprise.Api.Database;

public class AuthEnterpriseDbContext : DbContext
{
    public AuthEnterpriseDbContext(DbContextOptions<AuthEnterpriseDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthEnterpriseDbContext).Assembly);
    }
}
