using Conaprole.Orders.Application.Users.DeleteUser;
using FluentValidation.TestHelper;

namespace Conaprole.Orders.Application.UnitTests.Users.DeleteUser;

public class DeleteUserCommandValidatorTests
{
    private readonly DeleteUserCommandValidator _validator = new();

    [Fact]
    public void Validator_Should_HaveError_When_UserIdIsEmpty()
    {
        // Arrange
        var command = new DeleteUserCommand(Guid.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required");
    }

    [Fact]
    public void Validator_Should_NotHaveError_When_UserIdIsValid()
    {
        // Arrange
        var command = new DeleteUserCommand(Guid.NewGuid());

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}