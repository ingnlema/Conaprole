namespace Conaprole.Orders.Api.Controllers.Orders;

public record CreateOrderRequest(
    Guid PointOfSaleId,
    string Distributor,
    string City,
    string Street,
    string ZipCode,
    decimal Price,
    string CurrencyCode);