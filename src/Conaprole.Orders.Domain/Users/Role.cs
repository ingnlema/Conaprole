namespace Conaprole.Orders.Domain.Users;

public sealed class Role
{
    public static readonly Role Registered = new(1, "Registered");
    public static readonly Role API = new(2, "API");
    public static readonly Role Administrator = new(3, "Administrator");
    public static readonly Role Distributor = new(4, "Distributor");

    private Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public ICollection<User> Users { get; init; } = new List<User>();

    public ICollection<Permission> Permissions { get; init; } = new List<Permission>();
}