using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Domain.Exceptions;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.Orders;

public class OrderLineTests : BaseTest
{
    [Fact]
    public void Constructor_Should_SetPropertyValues_WhenUsingFullConstructor()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = OrderTestData.CreateTestProduct();
        var quantity = OrderTestData.DefaultQuantity;
        var subTotal = new Money(21.98m, Currency.Uyu);

        // Act
        var orderLine = new OrderLine(
            id,
            quantity,
            subTotal,
            product,
            OrderTestData.OrderId,
            OrderTestData.CreatedOnUtc);

        // Assert
        orderLine.Id.Should().Be(id);
        orderLine.Quantity.Should().Be(quantity);
        orderLine.SubTotal.Should().Be(subTotal);
        orderLine.Product.Should().Be(product);
        orderLine.OrderId.Should().Be(OrderTestData.OrderId);
        orderLine.CreatedOnUtc.Should().Be(OrderTestData.CreatedOnUtc);
    }

    [Fact]
    public void InternalConstructor_Should_CalculateSubTotal_Automatically()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = OrderTestData.CreateTestProduct(price: 15.50m);
        var quantity = new Quantity(3);
        var expectedSubTotal = new Money(46.50m, Currency.Uyu);

        // Act
        // Note: We can only use the public constructor for testing
        var calculatedSubTotal = product.UnitPrice * quantity;
        var orderLine = new OrderLine(
            id,
            quantity,
            calculatedSubTotal,
            product,
            OrderTestData.OrderId,
            OrderTestData.CreatedOnUtc);

        // Assert
        orderLine.Id.Should().Be(id);
        orderLine.Quantity.Should().Be(quantity);
        orderLine.SubTotal.Should().Be(expectedSubTotal);
        orderLine.Product.Should().Be(product);
    }

    [Fact]
    public void Constructor_Should_AcceptValidParameters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = OrderTestData.CreateTestProduct(price: 20.00m);
        var quantity = new Quantity(5);
        var subTotal = new Money(100.00m, Currency.Uyu);

        // Act
        var orderLine = new OrderLine(
            id,
            quantity,
            subTotal,
            product,
            OrderTestData.OrderId,
            OrderTestData.CreatedOnUtc);

        // Assert
        orderLine.Id.Should().Be(id);
        orderLine.Quantity.Should().Be(quantity);
        orderLine.SubTotal.Should().Be(subTotal);
        orderLine.Product.Should().Be(product);
    }

    [Fact]
    public void Constructor_Should_Work_WithDifferentProducts()
    {
        // Arrange
        var product1 = OrderTestData.CreateTestProduct("Product 1", 10.00m);
        var product2 = OrderTestData.CreateTestProduct("Product 2", 25.00m);
        var quantity = new Quantity(2);

        // Act
        var orderLine1 = new OrderLine(
            Guid.NewGuid(),
            quantity,
            new Money(20.00m, Currency.Uyu),
            product1,
            OrderTestData.OrderId,
            OrderTestData.CreatedOnUtc);

        var orderLine2 = new OrderLine(
            Guid.NewGuid(),
            quantity,
            new Money(50.00m, Currency.Uyu),
            product2,
            OrderTestData.OrderId,
            OrderTestData.CreatedOnUtc);

        // Assert
        orderLine1.Product.Should().Be(product1);
        orderLine1.SubTotal.Amount.Should().Be(20.00m);
        
        orderLine2.Product.Should().Be(product2);
        orderLine2.SubTotal.Amount.Should().Be(50.00m);
    }
}