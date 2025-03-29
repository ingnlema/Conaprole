namespace Conaprole.Orders.Application.Products.GetProducts;

public sealed class ProductsResponse
{
    public Guid Id { get; init; }
    public string ExternalProductId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime LastUpdated { get; init; }
    public List<string> Categories { get; set; } = new();
}