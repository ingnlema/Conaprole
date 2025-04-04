namespace Conaprole.Orders.Api.Controllers.Orders;

public record OrderLineRequest(
    string ExternalProductId,
    int Quantity);