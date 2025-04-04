namespace Conaprole.Orders.Application.Orders.CreateOrder;

public record CreateOrderLineCommand(
    string ExternalProductId,
    int Quantity);