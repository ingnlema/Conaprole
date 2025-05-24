using FluentValidation;

namespace Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale;

public class DisablePointOfSaleCommandValidator : AbstractValidator<DisablePointOfSaleCommand>
{
    public DisablePointOfSaleCommandValidator()
    {
        RuleFor(x => x.PointOfSalePhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20);
    }
}