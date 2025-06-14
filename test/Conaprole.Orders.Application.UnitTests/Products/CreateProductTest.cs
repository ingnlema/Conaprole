using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Products.CreateProduct;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;
using NSubstitute;

namespace Conaprole.Orders.Application.UnitTests.Products;

public class CreateProductTest
{
    private static readonly DateTime UtcNow = DateTime.Now;
    private static readonly CreateProductCommand Command = new CreateProductCommand(
        "SKU12345",
        "Test Product",
        100.50m,
        "UYU",
        "Test Description",
        Category.LACTEOS);

    private readonly CreateProductCommandHandler _handler;
    
    private readonly IProductRepository _productRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;

    public CreateProductTest()
    {
        _productRepositoryMock = Substitute.For<IProductRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        
        _dateTimeProviderMock.UtcNow.Returns(UtcNow);
        
        _handler = new CreateProductCommandHandler(
            _productRepositoryMock,
            _unitOfWorkMock,
            _dateTimeProviderMock);
    }
    
    [Fact]
    public async Task Handle_Should_CreateProduct_WhenValidData()
    {
        // Arrange
        _productRepositoryMock
            .GetByExternalIdAsync(Arg.Any<ExternalProductId>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        Product? capturedProduct = null;
        _productRepositoryMock
            .When(r => r.Add(Arg.Any<Product>()))
            .Do(ci => capturedProduct = ci.Arg<Product>());

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);
        
        _productRepositoryMock.Received(1).Add(Arg.Any<Product>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        
        Assert.NotNull(capturedProduct);
        Assert.Equal(Command.ExternalProductId, capturedProduct.ExternalProductId.Value);
        Assert.Equal(Command.Name, capturedProduct.Name.Value);
        Assert.Equal(Command.UnitPrice, capturedProduct.UnitPrice.Amount);
        Assert.Equal(Command.CurrencyCode, capturedProduct.UnitPrice.Currency.Code);
        Assert.Equal(Command.Description, capturedProduct.Description.Value);
        Assert.Equal(Command.Category, capturedProduct.Category);
        Assert.Equal(UtcNow, capturedProduct.LastUpdated);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenProductWithExternalIdAlreadyExists()
    {
        // Arrange
        var currency = Currency.FromCode(Command.CurrencyCode);
        var existingProduct = new Product(
            Guid.NewGuid(),
            new ExternalProductId(Command.ExternalProductId),
            new Name("Existing Product"),
            new Money(50, currency),
            Category.LACTEOS,
            new Description("Existing Description"),
            UtcNow);

        _productRepositoryMock
            .GetByExternalIdAsync(Arg.Any<ExternalProductId>(), Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.DuplicatedExternalId, result.Error);
        
        _productRepositoryMock.DidNotReceive().Add(Arg.Any<Product>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseValidEnumValues_WhenCreating()
    {
        // Arrange - Test that valid categories work correctly
        var commandWithValidCategory = new CreateProductCommand(
            "SKU99999",
            "Test Product",
            100.50m,
            "UYU",
            "Test Description",
            Category.CONGELADOS); // Valid category

        _productRepositoryMock
            .GetByExternalIdAsync(Arg.Any<ExternalProductId>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        Product? capturedProduct = null;
        _productRepositoryMock
            .When(r => r.Add(Arg.Any<Product>()))
            .Do(ci => capturedProduct = ci.Arg<Product>());

        // Act
        var result = await _handler.Handle(commandWithValidCategory, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(capturedProduct);
        Assert.Equal(Category.CONGELADOS, capturedProduct.Category);
        
        _productRepositoryMock.Received(1).Add(Arg.Any<Product>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseCorrectExternalProductId_WhenCreating()
    {
        // Arrange
        var externalProductId = new ExternalProductId(Command.ExternalProductId);
        
        _productRepositoryMock
            .GetByExternalIdAsync(externalProductId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsFailure);
        
        await _productRepositoryMock.Received(1).GetByExternalIdAsync(
            Arg.Is<ExternalProductId>(id => id.Value == Command.ExternalProductId),
            Arg.Any<CancellationToken>());
    }
}