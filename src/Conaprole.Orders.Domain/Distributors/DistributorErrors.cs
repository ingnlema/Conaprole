using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Distributors;

public static class DistributorErrors
{
    public static Error NotFound = new(
        "Distributor.NotFound",
        "The distributor with the specified identifier was not found.");

    public static Error AlreadyAssignedToPointOfSale = new(
        "Distributor.AlreadyAssigned",
        "The distributor is already assigned to the point of sale in the specified category.");

    public static readonly Error DistributorAlreadyAssigned = new(
        "Distributor.AlreadyAssignedToPOS",
        "The distributor has already been assigned to this point of sale and category.");

    public static readonly Error CategoryAlreadyAssigned = new(
        "Distributor.CategoryAlreadyAssigned",
        "The category is already assigned to the distributor.");

    public static readonly Error AlreadyExists = new(
        "Distributor.AlreadyExists",
        "A distributor with the specified phone number already exists.");

    public static readonly Error CategoryNotAssigned = new(
        "Distributor.CategoryNotAssigned",
        "The category is not currently assigned to the distributor.");
}