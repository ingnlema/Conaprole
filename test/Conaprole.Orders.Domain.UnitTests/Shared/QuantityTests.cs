using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Domain.Exceptions;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.Shared;

public class QuantityTests : BaseTest
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Constructor_Should_AcceptValidValues(int value)
    {
        // Act
        var quantity = new Quantity(value);

        // Assert
        quantity.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Constructor_Should_ThrowException_ForInvalidValues(int value)
    {
        // Act & Assert
        var act = () => new Quantity(value);
        act.Should().Throw<DomainException>()
           .WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void ImplicitConversion_ToInt_Should_Work()
    {
        // Arrange
        var quantity = new Quantity(5);

        // Act
        int value = quantity;

        // Assert
        value.Should().Be(5);
    }

    [Fact]
    public void ExplicitConversion_FromInt_Should_Work()
    {
        // Act
        var quantity = (Quantity)5;

        // Assert
        quantity.Value.Should().Be(5);
    }

    [Fact]
    public void ExplicitConversion_FromInt_Should_ThrowException_ForInvalidValues()
    {
        // Act & Assert
        var act = () => (Quantity)0;
        act.Should().Throw<DomainException>()
           .WithMessage("Quantity must be greater than zero.");
    }
}