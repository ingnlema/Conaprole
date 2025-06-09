using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class CreateOrderTest : BaseIntegrationTest
    {
        public CreateOrderTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task CreateOrderCommand_WithValidData_Returns_Success()
        {
            // 1) Sembrar datos necesarios
            var productId = await ProductData.SeedAsync(Sender);
            var distributorId = await DistributorData.SeedAsync(Sender);
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // 2) Crear el comando con los datos sembrados
            var command = new CreateOrderCommand(
                PointOfSaleData.PhoneNumber,
                DistributorData.PhoneNumber,
                "Montevideo",
                "Calle Test 123",
                "11200",
                "UYU",
                new List<CreateOrderLineCommand>
                {
                    new(ProductData.ExternalProductId, 2)
                }
            );

            // 3) Ejecutar el comando
            var result = await Sender.Send(command);

            // 4) Verificar que el resultado sea exitoso
            Assert.False(result.IsFailure);
            Assert.NotEqual(Guid.Empty, result.Value);

            // 5) Verificar que el pedido se creó correctamente consultándolo
            var createdOrderId = result.Value;
            var orderQuery = new GetOrderQuery(createdOrderId);
            var orderResult = await Sender.Send(orderQuery);
            
            Assert.False(orderResult.IsFailure);
            var order = orderResult.Value;
            
            // 6) Verificar que los datos del pedido coincidan con los sembrados
            Assert.Equal(PointOfSaleData.PhoneNumber, order.PointOfSalePhoneNumber);
            Assert.Equal(DistributorData.PhoneNumber, order.DistributorPhoneNumber);
            Assert.Equal("Montevideo", order.DeliveryAddressCity);
            Assert.Equal("Calle Test 123", order.DeliveryAddressStreet);
            Assert.Equal("11200", order.DeliveryAddressZipCode);
            Assert.Equal("UYU", order.PriceCurrency);
            Assert.Single(order.OrderLines);
            Assert.Equal(ProductData.ExternalProductId, order.OrderLines.First().Product.ExternalProductId);
            Assert.Equal(2, order.OrderLines.First().Quantity);
        }

        [Fact]
        public async Task CreateOrderCommand_WithInvalidPointOfSale_Returns_Failure()
        {
            // 1) Sembrar solo distribuidor y producto, pero no punto de venta
            var productId = await ProductData.SeedAsync(Sender);
            var distributorId = await DistributorData.SeedAsync(Sender);

            // 2) Crear comando con número de teléfono de punto de venta inexistente
            var command = new CreateOrderCommand(
                "+59899999999", // Número que no existe
                DistributorData.PhoneNumber,
                "Montevideo",
                "Calle Test 123",
                "11200",
                "UYU",
                new List<CreateOrderLineCommand>
                {
                    new(ProductData.ExternalProductId, 1)
                }
            );

            // 3) Ejecutar el comando
            var result = await Sender.Send(command);

            // 4) Verificar que el resultado sea un fallo
            Assert.True(result.IsFailure);
            Assert.Equal("Order.InvalidPointOfSale", result.Error.Code);
        }

        [Fact]
        public async Task CreateOrderCommand_WithInvalidDistributor_Returns_Failure()
        {
            // 1) Sembrar solo producto y punto de venta, pero no distribuidor
            var productId = await ProductData.SeedAsync(Sender);
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // 2) Crear comando con número de teléfono de distribuidor inexistente
            var command = new CreateOrderCommand(
                PointOfSaleData.PhoneNumber,
                "+59888888888", // Número que no existe
                "Montevideo",
                "Calle Test 123",
                "11200",
                "UYU",
                new List<CreateOrderLineCommand>
                {
                    new(ProductData.ExternalProductId, 1)
                }
            );

            // 3) Ejecutar el comando
            var result = await Sender.Send(command);

            // 4) Verificar que el resultado sea un fallo
            Assert.True(result.IsFailure);
            Assert.Equal("Order.InvalidDistributor", result.Error.Code);
        }

        [Fact]
        public async Task CreateOrderCommand_WithInvalidProduct_Returns_Failure()
        {
            // 1) Sembrar solo distribuidor y punto de venta, pero no producto
            var distributorId = await DistributorData.SeedAsync(Sender);
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // 2) Crear comando con ID de producto inexistente
            var command = new CreateOrderCommand(
                PointOfSaleData.PhoneNumber,
                DistributorData.PhoneNumber,
                "Montevideo",
                "Calle Test 123",
                "11200",
                "UYU",
                new List<CreateOrderLineCommand>
                {
                    new("INEXISTENT_PRODUCT", 1) // Producto que no existe
                }
            );

            // 3) Ejecutar el comando
            var result = await Sender.Send(command);

            // 4) Verificar que el resultado sea un fallo
            Assert.True(result.IsFailure);
            Assert.Equal("Product.NotFound", result.Error.Code);
        }
    }
}