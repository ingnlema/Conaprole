using FluentValidation;

namespace Conaprole.Orders.Application.Users.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty();

        // Keep consistent 6-character minimum for production security
        // The functional tests should be updated to use 6+ character passwords
        var minLength = 6;
        
        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .MinimumLength(minLength)
            .WithMessage($"Password must be at least {minLength} characters long");
    }
}