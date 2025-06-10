using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Orders.BulkCreateOrders;

internal sealed class BulkCreateOrdersCommandHandler : ICommandHandler<BulkCreateOrdersCommand, List<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IDistributorRepository _distributorRepository;

    public BulkCreateOrdersCommandHandler(
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

    public async Task<Result<List<Guid>>> Handle(BulkCreateOrdersCommand request, CancellationToken cancellationToken)
    {
        var orderIds = new List<Guid>();
        var createdOnUtc = _dateTimeProvider.UtcNow;

        // Process all orders in a single transaction
        // All validations and entity creation happens first
        foreach (var orderCommand in request.Orders)
        {
            var pointOfSale = await _pointOfSaleRepository.GetByPhoneNumberAsync(orderCommand.PointOfSalePhoneNumber, cancellationToken);
            if (pointOfSale is null)
                return Result.Failure<List<Guid>>(new Error("Order.InvalidPointOfSale", $"Point of Sale not found: {orderCommand.PointOfSalePhoneNumber}"));

            var distributor = await _distributorRepository.GetByPhoneNumberAsync(orderCommand.DistributorPhoneNumber, cancellationToken);
            if (distributor is null)
                return Result.Failure<List<Guid>>(new Error("Order.InvalidDistributor", $"Distributor not found: {orderCommand.DistributorPhoneNumber}"));
            
            var address = new Address(orderCommand.City, orderCommand.Street, orderCommand.ZipCode);
            var status = Status.Created;
            var currency = Currency.FromCode(orderCommand.CurrencyCode);
            
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

            foreach (var lineCommand in orderCommand.OrderLines)
            {
                var product = await _productRepository.GetByExternalIdAsync(new ExternalProductId(lineCommand.ExternalProductId), cancellationToken);
                if (product is null)
                    return Result.Failure<List<Guid>>(ProductErrors.NotFound);

                var quantity = new Quantity(lineCommand.Quantity);
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
            orderIds.Add(order.Id);
        }

        // Save all orders at once - if any fails, all are rolled back
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(orderIds);
    }
}