using Conaprole.Orders.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Conaprole.Orders.Infrastructure.Configuration;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });

        builder.HasData(
            // Registered role - basic user permissions + functional test permissions
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.UsersRead.Id
            },
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.OrdersRead.Id
            },
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.OrdersWrite.Id
            },
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.ProductsRead.Id
            },
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.ProductsWrite.Id
            },
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.PointsOfSaleRead.Id
            },
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.PointsOfSaleWrite.Id
            },
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.DistributorsRead.Id
            },
            new RolePermission
            {
                RoleId = Role.Registered.Id,
                PermissionId = Permission.DistributorsWrite.Id
            },

            // API role - full permissions for integrations and frontend
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.UsersRead.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.UsersWrite.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.DistributorsRead.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.DistributorsWrite.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.PointsOfSaleRead.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.PointsOfSaleWrite.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.ProductsRead.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.ProductsWrite.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.OrdersRead.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.OrdersWrite.Id },
            new RolePermission { RoleId = Role.API.Id, PermissionId = Permission.AdminAccess.Id },

            // Administrator role - full permissions for internal operations
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.UsersRead.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.UsersWrite.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.DistributorsRead.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.DistributorsWrite.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.PointsOfSaleRead.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.PointsOfSaleWrite.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.ProductsRead.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.ProductsWrite.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.OrdersRead.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.OrdersWrite.Id },
            new RolePermission { RoleId = Role.Administrator.Id, PermissionId = Permission.AdminAccess.Id },

            // Distributor role - limited access to distributors, points of sale, and read-only products
            new RolePermission { RoleId = Role.Distributor.Id, PermissionId = Permission.UsersRead.Id },
            new RolePermission { RoleId = Role.Distributor.Id, PermissionId = Permission.DistributorsRead.Id },
            new RolePermission { RoleId = Role.Distributor.Id, PermissionId = Permission.DistributorsWrite.Id },
            new RolePermission { RoleId = Role.Distributor.Id, PermissionId = Permission.PointsOfSaleRead.Id },
            new RolePermission { RoleId = Role.Distributor.Id, PermissionId = Permission.PointsOfSaleWrite.Id },
            new RolePermission { RoleId = Role.Distributor.Id, PermissionId = Permission.ProductsRead.Id },
            new RolePermission { RoleId = Role.Distributor.Id, PermissionId = Permission.OrdersRead.Id },
            new RolePermission { RoleId = Role.Distributor.Id, PermissionId = Permission.OrdersWrite.Id });
    }
}