using FluentValidation;

namespace Conaprole.Orders.Application.Users.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty();

        // Use 5 for test environments, 6 for production
        // This will be environment-dependent when IHostEnvironment is available in DI
        var minLength = 6; // Default to production standard
        
        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .MinimumLength(minLength)
            .WithMessage($"Password must be at least {minLength} characters long");
    }
}