using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Shared; 
using Conaprole.Orders.Domain.Products;
using MediatR;

namespace Conaprole.Orders.Application.Orders.AddOrderLine;

internal sealed class AddOrderLineToOrderCommandHandler : ICommandHandler<AddOrderLineToOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public AddOrderLineToOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }
    
    public async Task<Result<Guid>> Handle(AddOrderLineToOrderCommand request, CancellationToken cancellationToken)
    {

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(new Error("Order.NotFound", "El pedido no fue encontrado."));
        }
        

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<Guid>(new Error("Produc.tNotFound", "El producto no fue encontrado"));
        }
        
        var quantity = new Quantity(request.Quantity);
        var currency = Currency.FromCode(request.CurrencyCode);

        var subTotal = new Money(request.UnitPrice * request.Quantity, currency);
        
        var orderLine = new OrderLine(
            Guid.NewGuid(),
            quantity,
            subTotal,
            product,
            new OrderId(order.Id), 
            _dateTimeProvider.UtcNow
        );
        
        order.AddOrderLine(orderLine);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return orderLine.Id;
    }
}
