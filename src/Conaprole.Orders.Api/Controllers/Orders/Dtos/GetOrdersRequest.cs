namespace Conaprole.Orders.Api.Controllers.Orders;

public record GetOrdersRequest
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? Status { get; set; }
    public string? Distributor { get; set; }
    public string? PointOfSalePhoneNumber { get; set; }
    public string? Ids { get; set; }
}