using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Domain.Products;

namespace Conaprole.Orders.Domain.Orders;

public class OrderLine : Entity
{
    public OrderLine(
        Guid id, 
        Quantity quantity, 
        Money subTotal, 
        Product product, 
        OrderId orderId, 
        DateTime createdOnUtc) : base(id)
    {
        Quantity = quantity;
        SubTotal = subTotal;
        Product = product;
        OrderId = orderId;
        CreatedOnUtc = createdOnUtc;
    }
    
    public Quantity Quantity { get; private set; }
    public Money SubTotal { get; private set; }
    public OrderId OrderId { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public Product Product { get; private set; }

}