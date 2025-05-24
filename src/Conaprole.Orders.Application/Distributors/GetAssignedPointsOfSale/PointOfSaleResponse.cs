namespace Conaprole.Orders.Application.Distributors.GetAssignedPointsOfSale;

public class PointOfSaleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
}