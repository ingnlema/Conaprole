namespace Conaprole.Orders.Application.Orders.GetOrders;

public sealed class OrderSummaryResponse
{
    public Guid   Id                      { get; init; }
    public int    Status                  { get; init; }
    public string StatusName              { get; init; } = string.Empty;     
    public DateTime CreatedOnUtc          { get; init; }
    public string DistributorPhoneNumber { get; init; } = string.Empty;
    public string DistributorName         { get; init; } = string.Empty;
    public string PointOfSalePhoneNumber  { get; init; } = string.Empty;    
    public string City                    { get; init; } = string.Empty;     
    public string Street                  { get; init; } = string.Empty;     
    public string ZipCode                 { get; init; } = string.Empty;     
    public decimal PriceAmount            { get; init; }
    public string  PriceCurrency          { get; init; } = string.Empty;
}
