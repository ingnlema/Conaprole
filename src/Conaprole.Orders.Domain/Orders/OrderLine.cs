using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Exceptions;
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
    
    internal OrderLine(Guid id, Product product, Quantity quantity, OrderId orderId, DateTime createdOnUtc) : base(id)
    {
        Product = product ?? throw new DomainException("Product must be provided.");
        Quantity = quantity;
        SubTotal = product.UnitPrice * quantity;
        OrderId = orderId;
        CreatedOnUtc = createdOnUtc;
    }
    
    private OrderLine()
    {
        
    }
    public Quantity Quantity { get; private set; }
    public Money SubTotal { get; private set; }
    public OrderId OrderId { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public Product Product { get; private set; }
    
    internal void UpdateQuantity(Quantity newQuantity)
    {
        if (newQuantity is null) throw new DomainException("Quantity must be provided.");
        Quantity = newQuantity;
        SubTotal = Product.UnitPrice * newQuantity;
    }

}