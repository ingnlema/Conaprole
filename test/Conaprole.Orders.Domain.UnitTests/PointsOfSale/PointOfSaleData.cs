using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.UnitTests.PointsOfSale;

internal static class PointOfSaleData
{
    public static readonly string Name = "Test Point of Sale";
    public static readonly string PhoneNumber = "+59899987654";
    public static readonly Address Address = new("Montevideo", "Av. 18 de Julio 1234", "11200");
    public static readonly DateTime CreatedAt = new(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc);
}