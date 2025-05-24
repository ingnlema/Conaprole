namespace Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;

public record AssignDistributorToPointOfSaleRequest(
    string DistributorPhoneNumber,
    string Category
);