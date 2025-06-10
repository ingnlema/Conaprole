using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Orders.CreateOrder;

namespace Conaprole.Orders.Application.Orders.BulkCreateOrders;

public record BulkCreateOrdersCommand(
    List<CreateOrderCommand> Orders
) : ICommand<List<Guid>>;