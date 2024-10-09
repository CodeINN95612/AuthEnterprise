namespace AuthEnterprise.Api.Database;

public class UserRole
{
    public required Ulid UserId { get; set; }
    public required Ulid RoleId { get; set; }
    public required DateTime AssinedAt { get; set; }

    public required User User { get; set; }
    public required Role Role { get; set; }
}
