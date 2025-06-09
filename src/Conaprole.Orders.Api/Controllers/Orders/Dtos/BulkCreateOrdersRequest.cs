namespace Conaprole.Orders.Api.Controllers.Orders;

public record BulkCreateOrdersRequest
{
    public List<CreateOrderRequest> Orders { get; set; } = new();
}