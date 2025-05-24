using FluentValidation;

namespace Conaprole.Orders.Application.Distributors.RemoveCategory;

public class RemoveDistributorCategoryValidator : AbstractValidator<RemoveDistributorCategoryCommand>
{
    public RemoveDistributorCategoryValidator()
    {
        RuleFor(x => x.DistributorPhoneNumber)
            .NotEmpty().WithMessage("Distributor phone number is required.")
            .MaximumLength(20);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid category.");
    }
}