using Conaprole.Orders.Application.Orders.RemoveOrderLine;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Exceptions;
using Conaprole.Orders.Domain.Orders;
using NSubstitute;

namespace Conaprole.Orders.Application.UnitTests.Orders;

public class RemoveOrderLineFromOrderTest
{
    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly Guid OrderLineId = Guid.NewGuid();
    private static readonly RemoveOrderLineFromOrderCommand Command = new RemoveOrderLineFromOrderCommand(OrderId, OrderLineId);

    private readonly RemoveOrderLineFromOrderCommandHandler _handler;
    
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public RemoveOrderLineFromOrderTest()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        _handler = new RemoveOrderLineFromOrderCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock);
    }
    
    [Fact]
    public async Task Handle_Should_RemoveOrderLine_WhenOrderLineExists()
    {
        // Arrange
        _orderRepositoryMock
            .RemoveOrderLineAsync(OrderId, OrderLineId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(OrderLineId, result.Value);
        
        await _orderRepositoryMock.Received(1).RemoveOrderLineAsync(
            OrderId, 
            OrderLineId, 
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenOrderLineNotFound()
    {
        // Arrange
        _orderRepositoryMock
            .RemoveOrderLineAsync(OrderId, OrderLineId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.LineNotFound, result.Error);
        
        await _orderRepositoryMock.Received(1).RemoveOrderLineAsync(
            OrderId, 
            OrderLineId, 
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenLastOrderLineCannotBeRemoved()
    {
        // Arrange
        _orderRepositoryMock
            .RemoveOrderLineAsync(OrderId, OrderLineId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new DomainException("Cannot remove the last order line")));

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.LastOrderLineCannotBeRemoved, result.Error);
        
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}