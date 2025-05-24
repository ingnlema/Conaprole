using FluentValidation;

namespace Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;

public class CreatePointOfSaleCommandValidator : AbstractValidator<CreatePointOfSaleCommand>
{
    public CreatePointOfSaleCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(c => c.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20);

        RuleFor(c => c.City)
            .NotEmpty().WithMessage("City is required.");

        RuleFor(c => c.Street)
            .NotEmpty().WithMessage("Street is required.");

        RuleFor(c => c.ZipCode)
            .NotEmpty().WithMessage("ZipCode is required.");
    }
}