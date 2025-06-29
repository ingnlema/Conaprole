using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Infrastructure.Configuration.SeedData;

public static class RolePermissionMapping
{
    public static Dictionary<ApplicationRole, ApplicationPermission[]> GetRolePermissionMappings()
    {
        return new Dictionary<ApplicationRole, ApplicationPermission[]>
        {
            [ApplicationRole.Registered] = new[]
            {
                ApplicationPermission.ProductsRead,
                ApplicationPermission.OrdersRead,
                ApplicationPermission.OrdersWrite
            },
            [ApplicationRole.API] = new[]
            {
                ApplicationPermission.ProductsRead,
                ApplicationPermission.OrdersRead,
                ApplicationPermission.DistributorsRead,
                ApplicationPermission.PointsOfSaleRead
            },
            [ApplicationRole.Administrator] = Enum.GetValues<ApplicationPermission>(), // All permissions
            [ApplicationRole.Distributor] = new[]
            {
                ApplicationPermission.ProductsRead,
                ApplicationPermission.OrdersRead,
                ApplicationPermission.OrdersWrite,
                ApplicationPermission.PointsOfSaleRead
            }
        };
    }

    public static string GetPermissionName(ApplicationPermission permission)
    {
        return permission switch
        {
            ApplicationPermission.UsersRead => "users:read",
            ApplicationPermission.UsersWrite => "users:write",
            ApplicationPermission.DistributorsRead => "distributors:read",
            ApplicationPermission.DistributorsWrite => "distributors:write",
            ApplicationPermission.PointsOfSaleRead => "pointsofsale:read",
            ApplicationPermission.PointsOfSaleWrite => "pointsofsale:write",
            ApplicationPermission.ProductsRead => "products:read",
            ApplicationPermission.ProductsWrite => "products:write",
            ApplicationPermission.OrdersRead => "orders:read",
            ApplicationPermission.OrdersWrite => "orders:write",
            ApplicationPermission.AdminAccess => "admin:access",
            _ => permission.ToString().ToLowerInvariant()
        };
    }
}