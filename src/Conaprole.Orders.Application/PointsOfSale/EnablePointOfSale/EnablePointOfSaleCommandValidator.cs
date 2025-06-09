using FluentValidation;

namespace Conaprole.Orders.Application.PointsOfSale.EnablePointOfSale;

public class EnablePointOfSaleCommandValidator : AbstractValidator<EnablePointOfSaleCommand>
{
    public EnablePointOfSaleCommandValidator()
    {
        RuleFor(x => x.PointOfSalePhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20);
    }
}