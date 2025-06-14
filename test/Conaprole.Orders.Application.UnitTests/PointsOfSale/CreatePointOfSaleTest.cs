using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using PointOfSaleEntity = Conaprole.Orders.Domain.PointsOfSale.PointOfSale;
using Conaprole.Orders.Domain.Shared;
using NSubstitute;

namespace Conaprole.Orders.Application.UnitTests.PointsOfSale;

public class CreatePointOfSaleTest
{
    private static readonly DateTime UtcNow = DateTime.Now;
    private static readonly CreatePointOfSaleCommand Command = new CreatePointOfSaleCommand(
        "Test Point of Sale",
        "094123456",
        "Montevideo",
        "Test Street 123",
        "11100");

    private readonly CreatePointOfSaleCommandHandler _handler;
    
    private readonly IPointOfSaleRepository _pointOfSaleRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;

    public CreatePointOfSaleTest()
    {
        _pointOfSaleRepositoryMock = Substitute.For<IPointOfSaleRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        
        _dateTimeProviderMock.UtcNow.Returns(UtcNow);
        
        _handler = new CreatePointOfSaleCommandHandler(
            _pointOfSaleRepositoryMock,
            _unitOfWorkMock,
            _dateTimeProviderMock);
    }
    
    [Fact]
    public async Task Handle_Should_CreatePointOfSale_WhenValidData()
    {
        // Arrange
        _pointOfSaleRepositoryMock
            .GetByPhoneNumberAsync(Command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((PointOfSaleEntity?)null);

        PointOfSaleEntity? capturedPointOfSale = null;
        _pointOfSaleRepositoryMock
            .When(r => r.AddAsync(Arg.Any<PointOfSaleEntity>(), Arg.Any<CancellationToken>()))
            .Do(ci => capturedPointOfSale = ci.Arg<PointOfSaleEntity>());

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);
        
        await _pointOfSaleRepositoryMock.Received(1).AddAsync(Arg.Any<PointOfSaleEntity>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        
        Assert.NotNull(capturedPointOfSale);
        Assert.Equal(Command.Name, capturedPointOfSale.Name);
        Assert.Equal(Command.PhoneNumber, capturedPointOfSale.PhoneNumber);
        Assert.Contains(Command.City, capturedPointOfSale.Address);
        Assert.Contains(Command.Street, capturedPointOfSale.Address);
        Assert.Contains(Command.ZipCode, capturedPointOfSale.Address);
        Assert.Equal(UtcNow, capturedPointOfSale.CreatedAt);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenPointOfSaleWithPhoneNumberAlreadyExists()
    {
        // Arrange
        var existingPointOfSale = new PointOfSaleEntity(
            Guid.NewGuid(),
            "Existing POS",
            Command.PhoneNumber,
            new Address("Other City", "Other Street", "99999"),
            UtcNow);

        _pointOfSaleRepositoryMock
            .GetByPhoneNumberAsync(Command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns(existingPointOfSale);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PointOfSaleErrors.AlreadyExists, result.Error);
        
        await _pointOfSaleRepositoryMock.DidNotReceive().AddAsync(Arg.Any<PointOfSaleEntity>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseCorrectPhoneNumber_WhenChecking()
    {
        // Arrange
        _pointOfSaleRepositoryMock
            .GetByPhoneNumberAsync(Command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((PointOfSaleEntity?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        
        await _pointOfSaleRepositoryMock.Received(1).GetByPhoneNumberAsync(
            Command.PhoneNumber,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseCurrentDateTime_WhenCreating()
    {
        // Arrange
        _pointOfSaleRepositoryMock
            .GetByPhoneNumberAsync(Command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((PointOfSaleEntity?)null);

        PointOfSaleEntity? capturedPointOfSale = null;
        _pointOfSaleRepositoryMock
            .When(r => r.AddAsync(Arg.Any<PointOfSaleEntity>(), Arg.Any<CancellationToken>()))
            .Do(ci => capturedPointOfSale = ci.Arg<PointOfSaleEntity>());

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        
        // Verify DateTime provider was accessed
        var accessedUtcNow = _dateTimeProviderMock.Received().UtcNow;
        
        Assert.NotNull(capturedPointOfSale);
        Assert.Equal(UtcNow, capturedPointOfSale.CreatedAt);
    }
}