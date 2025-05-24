namespace Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;

public record CreatePointOfSaleRequest(
    string Name,
    string PhoneNumber,
    string City,
    string Street,
    string ZipCode
);