namespace Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

public record PointOfSaleResponse(
    Guid Id,
    string Name,
    string PhoneNumber,
    string Address,
    bool IsActive,
    DateTime CreatedAt
);