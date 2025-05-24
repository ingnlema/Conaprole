namespace Conaprole.Orders.Api.Controllers.Orders;

public record CreateOrderRequest(
    string PointOfSalePhoneNumber,
    string DistributorPhoneNumber,
    string City,
    string Street,
    string ZipCode,
    string CurrencyCode,
    List<OrderLineRequest> OrderLines);

