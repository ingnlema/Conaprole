using System.Threading;
using System.Threading.Tasks;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Abstractions;
using MediatR;

namespace Conaprole.Orders.Application.Orders.UpdateOrderStatus
{
    internal sealed class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, Unit>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateOrderStatusCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<Unit>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure<Unit>(new Error("Order.NotFound", "Product not found."));

            order.UpdateStatus(request.NewStatus, _dateTimeProvider.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(Unit.Value);
        }
    }
}