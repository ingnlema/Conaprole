using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.Shared;

public class MoneyTests : BaseTest
{
    [Fact]
    public void Constructor_Should_SetProperties()
    {
        // Arrange
        var amount = 100m;
        var currency = Currency.Uyu;

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Addition_Should_Work_WithSameCurrency()
    {
        // Arrange
        var money1 = new Money(50m, Currency.Uyu);
        var money2 = new Money(30m, Currency.Uyu);

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(80m);
        result.Currency.Should().Be(Currency.Uyu);
    }

    [Fact]
    public void Addition_Should_ThrowException_WithDifferentCurrencies()
    {
        // Arrange
        var money1 = new Money(50m, Currency.Uyu);
        var money2 = new Money(30m, Currency.Usd);

        // Act & Assert
        var act = () => money1 + money2;
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Currencies have to be equal");
    }

    [Fact]
    public void Subtraction_Should_Work_WithSameCurrency()
    {
        // Arrange
        var money1 = new Money(50m, Currency.Uyu);
        var money2 = new Money(30m, Currency.Uyu);

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(20m);
        result.Currency.Should().Be(Currency.Uyu);
    }

    [Fact]
    public void Subtraction_Should_ThrowException_WithDifferentCurrencies()
    {
        // Arrange
        var money1 = new Money(50m, Currency.Uyu);
        var money2 = new Money(30m, Currency.Usd);

        // Act & Assert
        var act = () => money1 - money2;
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Currencies have to be equal");
    }

    [Fact]
    public void Subtraction_Should_ThrowException_WhenResultWouldBeNegative()
    {
        // Arrange
        var money1 = new Money(30m, Currency.Uyu);
        var money2 = new Money(50m, Currency.Uyu);

        // Act & Assert
        var act = () => money1 - money2;
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Invalid Money operation");
    }

    [Fact]
    public void Multiplication_Should_Work_WithQuantity()
    {
        // Arrange
        var money = new Money(10m, Currency.Uyu);
        var quantity = new Quantity(3);

        // Act
        var result1 = money * quantity;
        var result2 = quantity * money;

        // Assert
        result1.Amount.Should().Be(30m);
        result1.Currency.Should().Be(Currency.Uyu);
        result2.Amount.Should().Be(30m);
        result2.Currency.Should().Be(Currency.Uyu);
    }

    [Fact]
    public void Zero_Should_ReturnZeroMoney()
    {
        // Act
        var zero = Money.Zero();

        // Assert
        zero.Amount.Should().Be(0);
        zero.IsZero().Should().BeTrue();
    }

    [Fact]
    public void Zero_WithCurrency_Should_ReturnZeroMoneyWithSpecifiedCurrency()
    {
        // Act
        var zero = Money.Zero(Currency.Uyu);

        // Assert
        zero.Amount.Should().Be(0);
        zero.Currency.Should().Be(Currency.Uyu);
    }

    [Fact]
    public void IsZero_Should_ReturnTrue_ForZeroAmount()
    {
        // Arrange
        var zero = Money.Zero(Currency.Uyu);

        // Act & Assert
        zero.IsZero().Should().BeTrue();
    }

    [Fact]
    public void IsZero_Should_ReturnFalse_ForNonZeroAmount()
    {
        // Arrange
        var money = new Money(10m, Currency.Uyu);

        // Act & Assert
        money.IsZero().Should().BeFalse();
    }
}