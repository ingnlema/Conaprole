namespace Conaprole.Orders.Application.PointsOfSale.GetDistributorsByPointOfSale;

public class DistributorWithCategoriesResponse
{
    public required string PhoneNumber { get; init; }
    public required string Name { get; init; }
    public List<string> Categories { get; init; } = new();
}