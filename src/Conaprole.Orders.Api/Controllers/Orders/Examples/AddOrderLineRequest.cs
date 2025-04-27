using System.ComponentModel.DataAnnotations;

namespace Conaprole.Orders.Api.Controllers.Orders.Examples;

public record AddOrderLineRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    [Required]
    public string CurrencyCode { get; init; }

    [Required]
    public decimal UnitPrice { get; init; }
}