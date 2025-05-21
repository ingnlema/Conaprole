
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Exceptions;
using Conaprole.Orders.Domain.Orders;


namespace Conaprole.Orders.Application.Orders.RemoveOrderLine
{
    internal sealed class RemoveOrderLineFromOrderCommandHandler
        : ICommandHandler<RemoveOrderLineFromOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork      _unitOfWork;

        public RemoveOrderLineFromOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork      = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RemoveOrderLineFromOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var removed = await _orderRepository.RemoveOrderLineAsync(
                    request.OrderId,
                    request.OrderLineId,
                    cancellationToken
                );

                if (!removed)
                    return Result.Failure<Guid>(OrderErrors.LineNotFound);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success(request.OrderLineId);
            }
            catch (DomainException ex) when (ex.Message.Contains("Cannot remove the last order line"))
            {
                return Result.Failure<Guid>(OrderErrors.LastOrderLineCannotBeRemoved);
            }
        }
    }
}