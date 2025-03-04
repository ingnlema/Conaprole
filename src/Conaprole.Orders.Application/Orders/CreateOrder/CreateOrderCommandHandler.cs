using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Shared;
using MediatR;

namespace Conaprole.Orders.Application.Orders.CreateOrder;

internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var pointOfSale = new PointOfSale(request.PointOfSaleId);
        var distributor = new Distributor(request.Distributor);
        var address = new Address(request.City, request.Street, request.ZipCode);
        var createdOnUtc = _dateTimeProvider.UtcNow;
        var status = Status.Created;
        
        var currency = Currency.FromCode(request.CurrencyCode);
        var price = new Money(request.Price, currency);

        var order = new Order(
            Guid.NewGuid(),
            pointOfSale,
            distributor,
            address,
            status,
            createdOnUtc,
            confirmedOnUtc: null,
            rejectedOnUtc: null,
            deliveryOnUtc: null,
            canceledOnUtc: null,
            deliveredOnUtc: null,
            price);

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
