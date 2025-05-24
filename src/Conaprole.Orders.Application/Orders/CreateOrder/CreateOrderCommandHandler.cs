using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.PointsOfSale;
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
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IDistributorRepository _distributorRepository;
    

    public CreateOrderCommandHandler(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IPointOfSaleRepository pointOfSaleRepository,
        IDistributorRepository distributorRepository)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _pointOfSaleRepository = pointOfSaleRepository;
        _distributorRepository = distributorRepository;
        
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var pointOfSale = await _pointOfSaleRepository.GetByPhoneNumberAsync(request.PointOfSalePhoneNumber, cancellationToken);
        if (pointOfSale is null)
            return Result.Failure<Guid>(new Error("Order.InvalidPointOfSale", "Point of Sale not found."));

        var distributor = await _distributorRepository.GetByPhoneNumberAsync(request.DistributorPhoneNumber, cancellationToken);
        if (distributor is null)
            return Result.Failure<Guid>(new Error("Order.InvalidDistributor", "Distributor not found."));
        var address = new Address(request.City, request.Street, request.ZipCode);
        var createdOnUtc = _dateTimeProvider.UtcNow;
        var status = Status.Created;
        var currency = Currency.FromCode(request.CurrencyCode);
        
        var order = new Order(
            Guid.NewGuid(),
            pointOfSale.Id,
            pointOfSale,
            distributor.Id,
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

        return Result.Success(order.Id);
    }
}
