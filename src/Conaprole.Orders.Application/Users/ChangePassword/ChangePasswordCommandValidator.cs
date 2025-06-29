using FluentValidation;
using Microsoft.Extensions.Hosting;

namespace Conaprole.Orders.Application.Users.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator(IHostEnvironment environment)
    {
        RuleFor(c => c.UserId)
            .NotEmpty();

        // Environment-based password length: 6 for production, 5 for test/dev
        var minLength = environment.IsProduction() ? 6 : 5;
        
        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .MinimumLength(minLength)
            .WithMessage($"Password must be at least {minLength} characters long");
    }
}