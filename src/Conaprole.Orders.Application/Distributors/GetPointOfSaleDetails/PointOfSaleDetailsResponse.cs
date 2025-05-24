namespace Conaprole.Orders.Application.Distributors.GetPointOfSaleDetails;

public class PointOfSaleDetailsResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }

    public List<CategoryAssignmentDto> DistributorCategories { get; init; } = new();
}

public class CategoryAssignmentDto
{
    public string DistributorPhoneNumber { get; init; } = string.Empty;
    public string DistributorName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}