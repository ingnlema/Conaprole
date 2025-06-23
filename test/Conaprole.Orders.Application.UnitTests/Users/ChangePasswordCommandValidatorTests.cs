using FluentValidation.TestHelper;
using Conaprole.Orders.Application.Users.ChangePassword;

namespace Conaprole.Orders.Application.UnitTests.Users;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordCommandValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.Empty, "password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Empty()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewPassword);
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Null()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewPassword);
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Too_Short()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "123"); // Less than 6 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewPassword)
            .WithErrorMessage("Password must be at least 6 characters long");
    }

    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Exactly_5_Characters()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "12345"); // Exactly 5 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.NewPassword)
            .WithErrorMessage("Password must be at least 6 characters long");
    }

    [Fact]
    public void Should_Not_Have_Error_When_NewPassword_Is_Exactly_6_Characters()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "123456"); // Exactly 6 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.NewPassword);
    }

    [Fact]
    public void Should_Not_Have_Error_When_NewPassword_Is_Valid()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "password123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.NewPassword);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }
}