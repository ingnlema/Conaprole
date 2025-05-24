using FluentValidation;

namespace Conaprole.Orders.Application.Distributors.CreateDistributor;

public class CreateDistributorCommandValidator : AbstractValidator<CreateDistributorCommand>
{
    public CreateDistributorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20);
    }
}