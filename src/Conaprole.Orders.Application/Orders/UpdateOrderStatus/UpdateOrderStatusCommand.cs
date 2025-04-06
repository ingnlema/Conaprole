using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Orders;
using MediatR;

namespace Conaprole.Orders.Application.Orders.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, Status NewStatus) : ICommand<Unit>;
