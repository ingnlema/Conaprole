namespace Conaprole.Orders.Application.Orders.GetOrder;

public class OrderLineResponse
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
    public decimal SubTotal { get; init; }
    public Guid OrderId { get; init; }
    public DateTime CreatedOnUtc { get; init; }
    public Guid ProductId { get; init; }
    public ProductResponse Product { get; init; } = new();
}