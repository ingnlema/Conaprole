namespace Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;

public record UnassignDistributorFromPointOfSaleRequest
{
    public string PointOfSalePhoneNumber { get; init; } = string.Empty;
    public string DistributorPhoneNumber { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}