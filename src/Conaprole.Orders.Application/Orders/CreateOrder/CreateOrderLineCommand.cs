namespace Conaprole.Orders.Application.Orders.CreateOrder;

public record CreateOrderLineCommand(
    Guid ProductId,
    int Quantity);