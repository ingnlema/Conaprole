
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Exceptions;
using Conaprole.Orders.Domain.Orders;


namespace Conaprole.Orders.Application.Orders.AddOrderLine
{
    internal sealed class AddOrderLineToOrderCommandHandler
        : ICommandHandler<AddOrderLineToOrderCommand, Guid>
    {
        private readonly IOrderRepository  _orderRepository;
        private readonly IUnitOfWork       _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AddOrderLineToOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _orderRepository  = orderRepository;
            _unitOfWork       = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<Guid>> Handle(
            AddOrderLineToOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var newLineId = await _orderRepository.AddOrderLineAsync(
                    request.OrderId,
                    request.ExternalProductId,
                    request.Quantity,
                    _dateTimeProvider.UtcNow,
                    cancellationToken
                );

                if (newLineId is null)
                    return Result.Failure<Guid>(new Error(
                        "Order.NotFound",
                        "Order or Product not found."));

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success(newLineId.Value);
            }
            catch (DomainException ex) when (ex.Message.Contains("already added"))
            {
                return Result.Failure<Guid>(new Error(
                    "OrderLine.Duplicate",
                    ex.Message));
            }
        }
    }
}
