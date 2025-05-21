
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Orders;


namespace Conaprole.Orders.Application.Orders.UpdateOrderLineQuantity
{
    internal sealed class UpdateOrderLineQuantityCommandHandler
        : ICommandHandler<UpdateOrderLineQuantityCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork      _unitOfWork;

        public UpdateOrderLineQuantityCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork      = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateOrderLineQuantityCommand request,
            CancellationToken cancellationToken)
        {
            var updated = await _orderRepository.UpdateOrderLineQuantityAsync(
                request.OrderId,
                request.OrderLineId,
                request.NewQuantity,
                cancellationToken
            );

            if (!updated)
                return Result.Failure<Guid>(OrderErrors.LineNotFound);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(request.OrderLineId);
        }
    }
}