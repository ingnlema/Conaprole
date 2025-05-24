using FluentValidation;

namespace Conaprole.Orders.Application.PointsOfSale.AssignDistributor;

public class AssignDistributorToPointOfSaleCommandValidator : AbstractValidator<AssignDistributorToPointOfSaleCommand>
{
    public AssignDistributorToPointOfSaleCommandValidator()
    {
        RuleFor(x => x.PointOfSalePhoneNumber)
            .NotEmpty().WithMessage("Point of Sale phone number is required.")
            .MaximumLength(20);

        RuleFor(x => x.DistributorPhoneNumber)
            .NotEmpty().WithMessage("Distributor phone number is required.")
            .MaximumLength(20);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category is not valid.");
    }
}