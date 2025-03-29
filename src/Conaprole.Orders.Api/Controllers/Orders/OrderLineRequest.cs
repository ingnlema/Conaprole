namespace Conaprole.Orders.Api.Controllers.Orders;

public record OrderLineRequest(
    Guid ProductId,
    int Quantity);