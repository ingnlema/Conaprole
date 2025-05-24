using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.PointsOfSale;

public static class PointOfSaleErrors
{
    public static readonly Error NotFound = new(
        "PointOfSale.NotFound",
        "The point of sale with the specified identifier was not found.");

    public static readonly Error InvalidCategory = new(
        "PointOfSale.InvalidCategory",
        "The specified category is invalid for this operation.");

    public static readonly Error AlreadyAssigned = new(
        "PointOfSale.AlreadyAssigned",
        "The distributor is already assigned to this point of sale for the given category.");

    public static readonly Error AssignmentFailed = new(
        "PointOfSale.AssignmentFailed",
        "The distributor could not be assigned to the point of sale.");
    
    
    public static readonly Error DistributorAlreadyAssigned = new(
        "Distributor.AlreadyAssignedToPOS",
        "The distributor has already been assigned to this point of sale and category.");

    public static readonly Error AlreadyExists = new(
        "PointOfSale.AlreadyExists",
        "A point of sale with the specified identifier already exists.");

    public static readonly Error AlreadyDisabled = new(
        "PointOfSale.AlreadyDisabled",
        "The point of sale is already disabled.");

    public static readonly Error DistributorNotAssigned = new(
        "PointOfSale.DistributorNotAssigned",
        "The distributor is not assigned to this point of sale for the given category.");

}