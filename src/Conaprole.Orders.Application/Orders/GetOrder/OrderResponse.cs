namespace Conaprole.Orders.Application.Orders.GetOrder;

public sealed class OrderResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    
    public Guid PointOfSaleId { get; init; }
    
    public required string Distributor { get; init; }
    
    public required string DeliveryAddressCity { get; init; }
    
    public required string DeliveryAddressStreet { get; init; }
    
    public required string DeliveryAddressZipCode { get; init; }
    
    public int Status { get; init; }
    
    public DateTime CreatedOnUtc { get; init; }
    
    public DateTime? ConfirmedOnUtc { get; init; }
    
    public DateTime? RejectedOnUtc { get; init; }
    
    public DateTime? DeliveryOnUtc { get; init; }
    
    public DateTime? CanceledOnUtc { get; init; }
    
    public DateTime? DeliveredOnUtc { get; init; }
    
    public decimal PriceAmount { get; init; }
    
    public required string PriceCurrency { get; init; }
}