using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

public record PointOfSaleResponse(
    Guid Id,
    string Name,
    string PhoneNumber,
    Address Address,
    bool IsActive,
    DateTime CreatedAt
);