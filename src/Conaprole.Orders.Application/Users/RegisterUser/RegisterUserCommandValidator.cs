using FluentValidation;

namespace Conaprole.Orders.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private const int MinPasswordLength = 5;

    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();

        RuleFor(c => c.LastName).NotEmpty();

        RuleFor(c => c.Email).EmailAddress();

        RuleFor(c => c.Password).NotEmpty().MinimumLength(MinPasswordLength);

        RuleFor(c => c.DistributorPhoneNumber)
            .MaximumLength(20)
            .When(c => !string.IsNullOrEmpty(c.DistributorPhoneNumber));
    }
}