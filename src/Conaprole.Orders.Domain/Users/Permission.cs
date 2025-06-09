namespace Conaprole.Orders.Domain.Users;

public sealed class Permission
{
    public static readonly Permission UsersRead = new(1, "users:read");
    public static readonly Permission UsersWrite = new(2, "users:write");
    public static readonly Permission DistributorsRead = new(3, "distributors:read");
    public static readonly Permission DistributorsWrite = new(4, "distributors:write");
    public static readonly Permission PointsOfSaleRead = new(5, "pointsofsale:read");
    public static readonly Permission PointsOfSaleWrite = new(6, "pointsofsale:write");
    public static readonly Permission ProductsRead = new(7, "products:read");
    public static readonly Permission ProductsWrite = new(8, "products:write");
    public static readonly Permission OrdersRead = new(9, "orders:read");
    public static readonly Permission OrdersWrite = new(10, "orders:write");
    public static readonly Permission AdminAccess = new(11, "admin:access");

    private Permission(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; }
}
