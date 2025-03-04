using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderResponse>;