using Conaprole.Orders.Application.Orders.UpdateOrderLineQuantity;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Orders;
using NSubstitute;

namespace Conaprole.Orders.Application.UnitTests.Orders;

public class UpdateOrderLineQuantityTest
{
    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly Guid OrderLineId = Guid.NewGuid();
    private static readonly UpdateOrderLineQuantityCommand Command = new UpdateOrderLineQuantityCommand(OrderId, OrderLineId, 10);

    private readonly UpdateOrderLineQuantityCommandHandler _handler;
    
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public UpdateOrderLineQuantityTest()
    {
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        _handler = new UpdateOrderLineQuantityCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock);
    }
    
    [Fact]
    public async Task Handle_Should_UpdateOrderLineQuantity_WhenOrderLineExists()
    {
        // Arrange
        _orderRepositoryMock
            .UpdateOrderLineQuantityAsync(OrderId, OrderLineId, Command.NewQuantity, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        Assert.Equal(OrderLineId, result.Value);
        
        await _orderRepositoryMock.Received(1).UpdateOrderLineQuantityAsync(
            OrderId, 
            OrderLineId, 
            Command.NewQuantity,
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenOrderLineNotFound()
    {
        // Arrange
        _orderRepositoryMock
            .UpdateOrderLineQuantityAsync(OrderId, OrderLineId, Command.NewQuantity, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.LineNotFound, result.Error);
        
        await _orderRepositoryMock.Received(1).UpdateOrderLineQuantityAsync(
            OrderId, 
            OrderLineId, 
            Command.NewQuantity,
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseCorrectQuantity_WhenUpdating()
    {
        // Arrange
        var newQuantity = 25;
        var commandWithDifferentQuantity = new UpdateOrderLineQuantityCommand(OrderId, OrderLineId, newQuantity);
        
        _orderRepositoryMock
            .UpdateOrderLineQuantityAsync(OrderId, OrderLineId, newQuantity, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(commandWithDifferentQuantity, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        
        await _orderRepositoryMock.Received(1).UpdateOrderLineQuantityAsync(
            OrderId, 
            OrderLineId, 
            newQuantity,
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}