namespace Conaprole.Orders.Application.Distributors.GetDistributors;

public sealed class DistributorSummaryResponse
{
    public Guid Id { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<string> SupportedCategories { get; init; } = new();
    public int AssignedPointsOfSaleCount { get; init; }
}