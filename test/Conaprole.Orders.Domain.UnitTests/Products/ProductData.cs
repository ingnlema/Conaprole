using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.UnitTests.Products;

internal static class ProductData
{
    public static readonly ExternalProductId ExternalProductId = new("EXT-001");
    public static readonly Name Name = new("Test Product");
    public static readonly Money UnitPrice = new(10.99m, Currency.Uyu);
    public static readonly Category Category = Category.LACTEOS;
    public static readonly Description Description = new("Test product description");
    public static readonly DateTime LastUpdated = new(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc);
}