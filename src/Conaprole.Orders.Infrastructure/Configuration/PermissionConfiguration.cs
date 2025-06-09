using Conaprole.Orders.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Conaprole.Orders.Infrastructure.Configuration;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(permission => permission.Id);

        builder.HasData(
            Permission.UsersRead,
            Permission.UsersWrite,
            Permission.DistributorsRead,
            Permission.DistributorsWrite,
            Permission.PointsOfSaleRead,
            Permission.PointsOfSaleWrite,
            Permission.ProductsRead,
            Permission.ProductsWrite,
            Permission.OrdersRead,
            Permission.OrdersWrite,
            Permission.AdminAccess);
    }
}