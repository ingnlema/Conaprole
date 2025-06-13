using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Distributors.GetAssignedPointsOfSale;

public class PointOfSaleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Address Address { get; init; } = new("", "", "");
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}