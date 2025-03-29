namespace Conaprole.Orders.Application.Orders.GetOrder;

public class ProductResponse
{
    public Guid Id { get; init; }
    public string ExternalProductId { get; set; }
    public string Name { get; init; }
    public decimal UnitPrice { get; init; }
    public string Description { get; init; }
    public List<string> Categories { get; init; } = new();
    public DateTime LastUpdated { get; init; }
}