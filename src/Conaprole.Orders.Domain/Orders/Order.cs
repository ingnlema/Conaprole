using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Exceptions;
using Conaprole.Orders.Domain.Orders.Events;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.Orders;

public class Order : Entity,IAggregateRoot
{
    
    private readonly List<OrderLine> _orderLines = new();
    
    public Order(
        Guid id,
        PointOfSale pointOfSale, 
        Distributor distributor,
        Address deliveryAddress, 
        Status status, 
        DateTime createdOnUtc, 
        DateTime? confirmedOnUtc, 
        DateTime? rejectedOnUtc, 
        DateTime? deliveryOnUtc,
        DateTime? canceledOnUtc, 
        DateTime? deliveredOnUtc, 
        Money price) : base(id)
    {
        PointOfSale = pointOfSale;
        Distributor = distributor;
        DeliveryAddress = deliveryAddress;
        Status = status;
        CreatedOnUtc = createdOnUtc;
        ConfirmedOnUtc = confirmedOnUtc;
        RejectedOnUtc = rejectedOnUtc;
        DeliveryOnUtc = deliveryOnUtc;
        CanceledOnUtc = canceledOnUtc;
        DeliveredOnUtc = deliveredOnUtc;
        Price = price;
    }

    private Order()
    {
        
    }
    public PointOfSale PointOfSale { get; private set; }
    public Distributor Distributor { get; private set; }
    public Status Status { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? ConfirmedOnUtc { get; private set; }
    public DateTime? RejectedOnUtc { get; private set; }
    public DateTime? DeliveryOnUtc { get; private set; }
    public DateTime? CanceledOnUtc { get; private set; }
    public DateTime? DeliveredOnUtc { get; private set; }
    public Address DeliveryAddress { get; private set; }
    
    public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();
    public Money Price { get; private set; }
    
    public void AddOrderLine(OrderLine orderLine)
    {
        if (orderLine == null)
            throw new DomainException("OrderLine must be provided.");

        var productId = orderLine.Product.Id;
        if (_orderLines.Any(l => l.Product.Id == productId))
            throw new DomainException("Product already added to order.");

        _orderLines.Add(orderLine);
        Price += orderLine.SubTotal;
        RaiseDomainEvent(new OrderLineAddedEvent(Id, orderLine.Id, productId, orderLine.Quantity.Value));
    }
    
    public void RemoveOrderLine(Guid productId)
    {
        var line = _orderLines.SingleOrDefault(l => l.Product.Id == productId)
                   ?? throw new DomainException("Order line not found.");

        _orderLines.Remove(line);
        Price -= line.SubTotal;
        RaiseDomainEvent(new OrderLineRemovedEvent(Id, line.Id, productId));
    }
    
    public void UpdateOrderLineQuantity(Guid productId, Quantity newQuantity)
    {
        var line = _orderLines.SingleOrDefault(l => l.Product.Id == productId)
                   ?? throw new DomainException("Order line not found.");

        var oldSubtotal = line.SubTotal;
        line.UpdateQuantity(newQuantity);
        Price = Price - oldSubtotal + line.SubTotal;
        RaiseDomainEvent(new OrderLineQuantityUpdatedEvent(Id, line.Id, newQuantity.Value));
    }
    
    public void UpdateStatus(Status newStatus, DateTime updateTime)
    {
        Status = newStatus;
        switch (newStatus)
        {
            case Status.Confirmed:
                ConfirmedOnUtc = updateTime;
                break;
            case Status.Delivered:
                DeliveredOnUtc = updateTime;
                break;
            case Status.Canceled:
                CanceledOnUtc = updateTime;
                break;
            case Status.Rejected:
                RejectedOnUtc = updateTime;
                break;
        }
    }
    
    

    
}