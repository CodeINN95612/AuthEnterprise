namespace AuthEnterprise.Api.Database;

public class Permission
{
    public required Ulid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Module { get; set; }
    public required string Action { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}