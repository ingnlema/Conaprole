namespace Conaprole.Orders.Api.Controllers.Orders;

/// <summary>
/// Request model for filtering and querying orders
/// </summary>
public record GetOrdersRequest
{
    /// <summary>
    /// Filter orders created from this date onwards (optional)
    /// </summary>
    public DateTime? From { get; set; }
    
    /// <summary>
    /// Filter orders created up to this date (optional)
    /// </summary>
    public DateTime? To { get; set; }
    
    /// <summary>
    /// Filter orders by status (optional)
    /// </summary>
    public int? Status { get; set; }
    
    /// <summary>
    /// Filter orders by distributor name (optional)
    /// </summary>
    public string? Distributor { get; set; }
    
    /// <summary>
    /// Filter orders by point of sale phone number (optional)
    /// </summary>
    public string? PointOfSalePhoneNumber { get; set; }
    
    /// <summary>
    /// Comma-separated list of order IDs to retrieve specific orders (optional)
    /// </summary>
    public string? Ids { get; set; }
}