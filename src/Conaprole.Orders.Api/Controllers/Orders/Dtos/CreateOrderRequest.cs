namespace Conaprole.Orders.Api.Controllers.Orders;

/// <summary>
/// Request model for creating a new order
/// </summary>
/// <param name="PointOfSalePhoneNumber">Phone number of the point of sale placing the order</param>
/// <param name="DistributorPhoneNumber">Phone number of the distributor fulfilling the order</param>
/// <param name="City">Delivery city</param>
/// <param name="Street">Delivery street address</param>
/// <param name="ZipCode">Delivery postal code</param>
/// <param name="CurrencyCode">Currency code for pricing (e.g., UYU, USD)</param>
/// <param name="OrderLines">List of product lines to include in the order</param>
public record CreateOrderRequest(
    string PointOfSalePhoneNumber,
    string DistributorPhoneNumber,
    string City,
    string Street,
    string ZipCode,
    string CurrencyCode,
    List<OrderLineRequest> OrderLines);

