using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Orders.UpdateOrderStatus;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.PointsOfSale;
using PointOfSaleEntity = Conaprole.Orders.Domain.PointsOfSale.PointOfSale;
using Conaprole.Orders.Domain.Shared;
using MediatR;
using NSubstitute;

namespace Conaprole.Orders.Application.UnitTests.Orders;

public class UpdateOrderStatusTest
{
    private static readonly DateTime UtcNow = DateTime.Now;
    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly UpdateOrderStatusCommand Command = new UpdateOrderStatusCommand(OrderId, Status.Confirmed);

    private readonly UpdateOrderStatusCommandHandler _handler;
    
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;

    public UpdateOrderStatusTest()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        
        _dateTimeProviderMock.UtcNow.Returns(UtcNow);
        
        _handler = new UpdateOrderStatusCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock,
            _dateTimeProviderMock);
    }
    
    [Fact]
    public async Task Handle_Should_UpdateOrderStatus_WhenOrderExists()
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

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(Unit.Value, result.Value);
        
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(Status.Confirmed, order.Status);
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
        
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_CallDateTimeProvider_WhenUpdatingStatus()
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

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        
        // Verify that the DateTime provider was called
        var accessedUtcNow = _dateTimeProviderMock.Received().UtcNow;
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}