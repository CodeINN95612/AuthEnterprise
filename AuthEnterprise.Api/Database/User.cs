namespace AuthEnterprise.Api.Database;

public class User
{
    public required Ulid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required bool IsActive { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}