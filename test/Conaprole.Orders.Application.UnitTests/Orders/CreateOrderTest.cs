using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.PointsOfSale;
using PointOfSaleEntity = Conaprole.Orders.Domain.PointsOfSale.PointOfSale;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;
using NSubstitute;

namespace Conaprole.Orders.Application.UnitTests.Orders;

public class CreateOrderTest{

    private static readonly DateTime UtcNow = DateTime.Now;
    private static readonly CreateOrderCommand Command = new CreateOrderCommand(
        "094000000",        
        "+59890000000",     
        "Montevideo",     
        "calle A",       
        "11100",          
        "UYU",    
        new List<CreateOrderLineCommand>
        {
            new CreateOrderLineCommand("SKU12345", 2),
            new CreateOrderLineCommand("SKU67890", 3)
        }
    );

    private readonly CreateOrderCommandHandler _handler;
    
    private readonly IProductRepository _productRepositoryMock;
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;
    private readonly IPointOfSaleRepository _pointOfSaleRepositoryMock;
    private readonly IDistributorRepository _distributorRepositoryMock;

    public CreateOrderTest()
    {
        _productRepositoryMock = Substitute.For<IProductRepository>();
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _pointOfSaleRepositoryMock = Substitute.For<IPointOfSaleRepository>();
        _distributorRepositoryMock = Substitute.For<IDistributorRepository>();
        
        _dateTimeProviderMock.UtcNow.Returns(UtcNow);
        
        _handler = new CreateOrderCommandHandler(
            _productRepositoryMock,
            _orderRepositoryMock,
            _unitOfWorkMock,
            _dateTimeProviderMock,
            _pointOfSaleRepositoryMock,
            _distributorRepositoryMock);
    }
    
    [Fact]
        public async Task Create_Should_SetPropertyValues()
        {

            var currency = Currency.FromCode("UYU");

            var product1 = new Product(
                Guid.NewGuid(),
                new ExternalProductId("SKU12345"),
                new Name("Producto A"),
                new Money(100, currency),
                Category.LACTEOS,
                new Description("Descripcion A"),
                UtcNow
            );

            var product2 = new Product(
                Guid.NewGuid(),
                new ExternalProductId("SKU67890"),
                new Name("Producto B"),
                new Money(50, currency),
                Category.LACTEOS,
                new Description("Descripcion B"),
                UtcNow
            );

            _productRepositoryMock
                .GetByExternalIdAsync(Arg.Is<ExternalProductId>(id => id.Value == "SKU12345"), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product1));

            _productRepositoryMock
                .GetByExternalIdAsync(Arg.Is<ExternalProductId>(id => id.Value == "SKU67890"), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product2));
            
            var pos = new PointOfSaleEntity(
                Guid.NewGuid(),
                "Punto A",
                Command.PointOfSalePhoneNumber,
                new Address(Command.City, Command.Street, Command.ZipCode),
                UtcNow
            );
       var dist = new Distributor(
           Guid.NewGuid(),
           "Nombre",
           Command.DistributorPhoneNumber,
           "Dirección A",
           UtcNow,
           new List<Category> { Category.LACTEOS });

            _pointOfSaleRepositoryMock
                .GetByPhoneNumberAsync(Command.PointOfSalePhoneNumber, Arg.Any<CancellationToken>())
                .Returns(pos);

            _distributorRepositoryMock
                .GetByPhoneNumberAsync(Command.DistributorPhoneNumber, Arg.Any<CancellationToken>())
                .Returns(dist);

            Order? capturedOrder = null;
            _orderRepositoryMock
                .When(r => r.Add(Arg.Any<Order>()))
                .Do(ci => capturedOrder = ci.Arg<Order>());


            var result = await _handler.Handle(Command, CancellationToken.None);
            
            Assert.False(result.IsFailure);
            Assert.NotEqual(Guid.Empty, result.Value);
            
            _orderRepositoryMock.Received(1).Add(Arg.Any<Order>());
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            
            Assert.NotNull(capturedOrder);
            Assert.Equal(Command.PointOfSalePhoneNumber, capturedOrder.PointOfSale.PhoneNumber);
            Assert.Equal("Punto A", capturedOrder.PointOfSale.Name);
            Assert.Equal(Command.DistributorPhoneNumber, capturedOrder.Distributor.PhoneNumber);
            Assert.Equal(Command.City, capturedOrder.DeliveryAddress.City);
            Assert.Equal(Command.Street, capturedOrder.DeliveryAddress.Street);
            Assert.Equal(Command.ZipCode, capturedOrder.DeliveryAddress.ZipCode);
            Assert.Equal(Status.Created, capturedOrder.Status);
            Assert.Equal(UtcNow, capturedOrder.CreatedOnUtc);


            var expectedTotal = new Money(200 + 150, currency);
            Assert.Equal(expectedTotal, capturedOrder.Price);


            Assert.Equal(2, capturedOrder.OrderLines.Count);
        }

        [Fact]
        public async Task Create_Should_ReturnFailure_WhenProductNotFound()
        {

            var currency = Currency.FromCode("UYU");

            var product1 = new Product(
                Guid.NewGuid(),
                new ExternalProductId("SKU12345"),
                new Name("Producto A"),
                new Money(100, currency),
                Category.LACTEOS,
                new Description("Descripcion A"),
                UtcNow
            );

            _productRepositoryMock
                .GetByExternalIdAsync(Arg.Is<ExternalProductId>(id => id.Value == "SKU12345"), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product1));

            _productRepositoryMock
                .GetByExternalIdAsync(Arg.Is<ExternalProductId>(id => id.Value == "SKU67890"), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(null));

            var pos = new PointOfSaleEntity(
                Guid.NewGuid(),
                "Punto A",
                Command.PointOfSalePhoneNumber,
                new Address(Command.City, Command.Street, Command.ZipCode),
                UtcNow
            );
            var dist = new Distributor(
                Guid.NewGuid(),
                "Nombre",
                Command.DistributorPhoneNumber,
                "Dirección A",
                UtcNow,
                new List<Category> { Category.LACTEOS });

            _pointOfSaleRepositoryMock
                .GetByPhoneNumberAsync(Command.PointOfSalePhoneNumber, Arg.Any<CancellationToken>())
                .Returns(pos);

            _distributorRepositoryMock
                .GetByPhoneNumberAsync(Command.DistributorPhoneNumber, Arg.Any<CancellationToken>())
                .Returns(dist);

            var result = await _handler.Handle(Command, CancellationToken.None);
            
            Assert.True(result.IsFailure);
            Assert.Equal(ProductErrors.NotFound, result.Error);
            
            _orderRepositoryMock.DidNotReceive().Add(Arg.Any<Order>());
            await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
        
        [Fact]
        public async Task Handle_Should_CallRepository_WhenOrderCreated()
        {
            var currency = Currency.FromCode("UYU");

            var product1 = new Product(
                Guid.NewGuid(),
                new ExternalProductId("SKU12345"),
                new Name("Producto A"),
                new Money(100, currency),
                Category.LACTEOS,
                new Description("Descripción A"),
                UtcNow
            );

            var product2 = new Product(
                Guid.NewGuid(),
                new ExternalProductId("SKU67890"),
                new Name("Producto B"),
                new Money(50, currency),
                Category.LACTEOS,
                new Description("Descripción B"),
                UtcNow
            );

            _productRepositoryMock
                .GetByExternalIdAsync(Arg.Is<ExternalProductId>(id => id.Value == "SKU12345"), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product1));

            _productRepositoryMock
                .GetByExternalIdAsync(Arg.Is<ExternalProductId>(id => id.Value == "SKU67890"), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Product?>(product2));
            
            var pos = new PointOfSaleEntity(
                Guid.NewGuid(),
                "Punto A",
                Command.PointOfSalePhoneNumber,
                new Address(Command.City, Command.Street, Command.ZipCode),
                UtcNow
            );
            var dist = new Distributor(
                Guid.NewGuid(),
                "Nombre",
                Command.DistributorPhoneNumber,
                "Dirección A",
                UtcNow,
                new List<Category> { Category.LACTEOS });

            _pointOfSaleRepositoryMock
                .GetByPhoneNumberAsync(Command.PointOfSalePhoneNumber, Arg.Any<CancellationToken>())
                .Returns(pos);

            _distributorRepositoryMock
                .GetByPhoneNumberAsync(Command.DistributorPhoneNumber, Arg.Any<CancellationToken>())
                .Returns(dist);

            Order? capturedOrder = null;
            _orderRepositoryMock
                .When(r => r.Add(Arg.Any<Order>()))
                .Do(ci => capturedOrder = ci.Arg<Order>());

            var result = await _handler.Handle(Command, CancellationToken.None);
            
            _orderRepositoryMock.Received(1).Add(Arg.Any<Order>());
            await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            Assert.NotNull(capturedOrder);
            Assert.Equal("Punto A", capturedOrder.PointOfSale.Name);
        }

    
}