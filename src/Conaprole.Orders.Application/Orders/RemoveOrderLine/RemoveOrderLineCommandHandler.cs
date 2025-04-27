using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Orders;

namespace Conaprole.Orders.Application.Orders.RemoveOrderLine;

internal sealed class RemoveOrderLineCommandHandler : ICommandHandler<RemoveOrderLineCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveOrderLineCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveOrderLineCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("Order.NotFound", "Order not found."));

        order.RemoveOrderLine(request.ProductId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}