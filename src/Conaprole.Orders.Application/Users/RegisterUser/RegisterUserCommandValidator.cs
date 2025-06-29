using FluentValidation;
using Microsoft.Extensions.Hosting;

namespace Conaprole.Orders.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator(IHostEnvironment environment)
    {
        RuleFor(c => c.FirstName).NotEmpty();

        RuleFor(c => c.LastName).NotEmpty();

        RuleFor(c => c.Email).EmailAddress();

        // Environment-based password length: 6 for production, 5 for test/dev
        var minLength = environment.IsProduction() ? 6 : 5;
        RuleFor(c => c.Password).NotEmpty().MinimumLength(minLength);

        RuleFor(c => c.DistributorPhoneNumber)
            .MaximumLength(20)
            .When(c => !string.IsNullOrEmpty(c.DistributorPhoneNumber));
    }
}