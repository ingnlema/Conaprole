using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Orders.AddOrderLine;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Exceptions;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.PointsOfSale;
using PointOfSaleEntity = Conaprole.Orders.Domain.PointsOfSale.PointOfSale;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;
using NSubstitute;

namespace Conaprole.Orders.Application.UnitTests.Orders;

public class AddOrderLineToOrderTest
{
    private static readonly DateTime UtcNow = DateTime.Now;
    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly ExternalProductId ExternalProductId = new ExternalProductId("SKU12345");
    private static readonly AddOrderLineToOrderCommand Command = new AddOrderLineToOrderCommand(OrderId, ExternalProductId, 5);

    private readonly AddOrderLineToOrderCommandHandler _handler;
    
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;

    public AddOrderLineToOrderTest()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        
        _dateTimeProviderMock.UtcNow.Returns(UtcNow);
        
        _handler = new AddOrderLineToOrderCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock,
            _dateTimeProviderMock);
    }
    
    [Fact]
    public async Task Handle_Should_AddOrderLine_WhenOrderExists()
    {
        // Arrange
        var currency = Currency.FromCode("UYU");
        var pos = new PointOfSaleEntity(
            Guid.NewGuid(),
            "Test POS",
            "094000000",
            new Address("Montevideo", "Test Street", "11100"),
            UtcNow);
        
        var distributor = new Distributor(
            Guid.NewGuid(),
            "Test Distributor",
            "+59890000000",
            "Test Address",
            UtcNow,
            new List<Category> { Category.LACTEOS });

        var order = new Order(
            OrderId,
            pos.Id,
            pos,
            distributor.Id,
            distributor,
            new Address("Montevideo", "Test Street", "11100"),
            Status.Created,
            UtcNow,
            null,
            null,
            null,
            null,
            null,
            new Money(100, currency));

        var newLineId = Guid.NewGuid();

        _orderRepositoryMock
            .GetByIdAsync(OrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        _orderRepositoryMock
            .AddOrderLineAsync(OrderId, ExternalProductId, Command.Quantity, UtcNow, Arg.Any<CancellationToken>())
            .Returns(newLineId);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(newLineId, result.Value);
        
        await _orderRepositoryMock.Received(1).AddOrderLineAsync(
            OrderId, 
            ExternalProductId, 
            Command.Quantity, 
            UtcNow, 
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        _orderRepositoryMock
            .GetByIdAsync(OrderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.NotFound, result.Error);
        
        await _orderRepositoryMock.DidNotReceive().AddOrderLineAsync(
            Arg.Any<Guid>(), 
            Arg.Any<ExternalProductId>(), 
            Arg.Any<int>(), 
            Arg.Any<DateTime>(), 
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenProductNotFound()
    {
        // Arrange
        var currency = Currency.FromCode("UYU");
        var pos = new PointOfSaleEntity(
            Guid.NewGuid(),
            "Test POS",
            "094000000",
            new Address("Montevideo", "Test Street", "11100"),
            UtcNow);
        
        var distributor = new Distributor(
            Guid.NewGuid(),
            "Test Distributor",
            "+59890000000",
            "Test Address",
            UtcNow,
            new List<Category> { Category.LACTEOS });

        var order = new Order(
            OrderId,
            pos.Id,
            pos,
            distributor.Id,
            distributor,
            new Address("Montevideo", "Test Street", "11100"),
            Status.Created,
            UtcNow,
            null,
            null,
            null,
            null,
            null,
            new Money(100, currency));

        _orderRepositoryMock
            .GetByIdAsync(OrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        _orderRepositoryMock
            .AddOrderLineAsync(OrderId, ExternalProductId, Command.Quantity, UtcNow, Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.ProductNotFound, result.Error);
        
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenDuplicateProductInOrder()
    {
        // Arrange
        var currency = Currency.FromCode("UYU");
        var pos = new PointOfSaleEntity(
            Guid.NewGuid(),
            "Test POS",
            "094000000",
            new Address("Montevideo", "Test Street", "11100"),
            UtcNow);
        
        var distributor = new Distributor(
            Guid.NewGuid(),
            "Test Distributor",
            "+59890000000",
            "Test Address",
            UtcNow,
            new List<Category> { Category.LACTEOS });

        var order = new Order(
            OrderId,
            pos.Id,
            pos,
            distributor.Id,
            distributor,
            new Address("Montevideo", "Test Street", "11100"),
            Status.Created,
            UtcNow,
            null,
            null,
            null,
            null,
            null,
            new Money(100, currency));

        _orderRepositoryMock
            .GetByIdAsync(OrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        _orderRepositoryMock
            .AddOrderLineAsync(OrderId, ExternalProductId, Command.Quantity, UtcNow, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Guid?>(new DomainException("Product already added to this order")));

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.DuplicateProductInOrder, result.Error);
        
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}