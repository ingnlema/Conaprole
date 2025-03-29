namespace Conaprole.Orders.Application.Orders.GetOrders;

public sealed class OrderSummaryResponse
{
    public Guid Id { get; init; }
    public int Status { get; init; }
    public DateTime CreatedOnUtc { get; init; }
    public string Distributor { get; init; } = string.Empty;
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = string.Empty;
}
