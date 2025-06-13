using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.UnitTests.Orders;

internal static class OrderTestData
{
    public static readonly OrderId OrderId = new(Guid.NewGuid());
    public static readonly DateTime CreatedOnUtc = new(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    
    public static Product CreateTestProduct(string name = "Test Product", decimal price = 10.99m)
    {
        return new Product(
            Guid.NewGuid(),
            new ExternalProductId("EXT-001"),
            new Name(name),
            new Money(price, Currency.Uyu),
            Category.LACTEOS,
            new Description("Test product description"),
            CreatedOnUtc);
    }
    
    public static Quantity DefaultQuantity => new(2);
}