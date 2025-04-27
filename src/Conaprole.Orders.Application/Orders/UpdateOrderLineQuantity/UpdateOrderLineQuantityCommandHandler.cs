using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Orders.UpdateOrderLineQuantity;

internal sealed class UpdateOrderLineQuantityCommandHandler : ICommandHandler<UpdateOrderLineQuantityCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderLineQuantityCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateOrderLineQuantityCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(new Error("Order.NotFound", "Order not found."));

        order.UpdateOrderLineQuantity(request.ProductId, new Quantity(request.NewQuantity));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}