using FluentValidation;

namespace Conaprole.Orders.Application.Distributors.AddCategory;

public class AddDistributorCategoryCommandValidator : AbstractValidator<AddDistributorCategoryCommand>
{
    public AddDistributorCategoryCommandValidator()
    {
        RuleFor(x => x.DistributorPhoneNumber)
            .NotEmpty().WithMessage("Distributor phone number is required.")
            .MaximumLength(20);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid category.");
    }
}