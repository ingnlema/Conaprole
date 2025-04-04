using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Domain.Users;
using MediatR;

namespace Conaprole.Orders.Application.Orders.CreateOrder;

internal sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateOrderCommandHandler(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var pointOfSale = new PointOfSale(request.PointOfSalePhoneNumber);
        var distributor = new Distributor(request.Distributor);
        var address = new Address(request.City, request.Street, request.ZipCode);
        var createdOnUtc = _dateTimeProvider.UtcNow;
        var status = Status.Created;
        var currency = Currency.FromCode(request.CurrencyCode);
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
            new Money(0, currency));

        foreach (var line in request.OrderLines)
        {
            var product = await _productRepository.GetByExternalIdAsync(new ExternalProductId(line.ExternalProductId), cancellationToken);
            if (product is null)
                return Result.Failure<Guid>(ProductErrors.NotFound);

            var quantity = new Quantity(line.Quantity);
            var subtotal = product.UnitPrice * quantity;
            var orderLine = new OrderLine(
                Guid.NewGuid(),
                quantity,
                subtotal,
                product,
                new OrderId(order.Id),
                createdOnUtc);

            order.AddOrderLine(orderLine);
        }

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
