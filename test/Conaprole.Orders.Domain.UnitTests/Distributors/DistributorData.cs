using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.UnitTests.Distributors;

internal static class DistributorData
{
    public static readonly string Name = "Test Distributor";
    public static readonly string PhoneNumber = "+59899123456";
    public static readonly string Address = "123 Test Street, Montevideo, Uruguay";
    public static readonly DateTime CreatedAt = new(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    public static readonly IEnumerable<Category> SupportedCategories = new[] { Category.LACTEOS, Category.CONGELADOS };
}