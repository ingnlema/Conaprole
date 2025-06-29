using FluentValidation;

namespace Conaprole.Orders.Application.Users.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    private const int MinLength = 5;

    public ChangePasswordCommandValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty();

        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .MinimumLength(MinLength)
            .WithMessage($"Password must be at least {MinLength} characters long");
    }
}