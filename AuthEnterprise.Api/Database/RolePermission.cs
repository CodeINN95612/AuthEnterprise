namespace AuthEnterprise.Api.Database;

public class RolePermission
{
    public required Ulid RoleId { get; set; }
    public required Ulid PermissionId { get; set; }
    public required DateTime AssinedAt { get; set; }

    public required Role Role { get; set; }
    public required Permission Permission { get; set; }
}
