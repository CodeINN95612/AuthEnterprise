namespace AuthEnterprise.Api.Database;

public class Permission
{
    private Permission() { }

    public static Permission Create(string code, string name, string description)
    {
        return new Permission
        {
            Id = Ulid.NewUlid(),
            Code = code,
            Name = name,
            Description = description,
            Module = code.Split('.')[0],
            Action = code.Split('.')[1],
        };
    }

    public Ulid Id { get; private set; } = Ulid.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}