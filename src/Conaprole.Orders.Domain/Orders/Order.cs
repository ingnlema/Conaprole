using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Domain.Orders;

public class Order : Entity
{
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
    
    public List<OrderLine> OrderLines { get; private set; } = new();
    public Money Price { get; private set; }
    
    public void AddOrderLine(OrderLine orderLine) {
         OrderLines.Add(orderLine);
        Price += orderLine.SubTotal;
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