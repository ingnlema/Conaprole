namespace Conaprole.Orders.Application.Products.GetProduct;

public sealed class ProductResponse
{
    public Guid Id { get; init; }
    public required string ExternalProductId { get; init; } = null!;
    public required string Name { get; init; } = null!;
    public decimal UnitPriceAmount { get; init; }
    public required string UnitPriceCurrency { get; init; } = null!;
    public required string Description { get; init; } = null!;
    public required List<string> Categories { get; set; } = null!;
    public DateTime LastUpdated { get; init; }
}
