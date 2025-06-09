using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Application.Orders.BulkCreateOrders;
using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.Orders.GetOrder;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class BulkCreateOrdersTest : BaseIntegrationTest
    {
        public BulkCreateOrdersTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task BulkCreateOrders_Successfully_Creates_Multiple_Orders()
        {
            // Arrange - Seed dependencies first
            await OrderData.SeedDependenciesAsync(Sender);

            var orders = new List<CreateOrderCommand>
            {
                new CreateOrderCommand(
                    OrderData.PointOfSalePhone,
                    OrderData.DistributorPhone,
                    "City1",
                    "Street1", 
                    "12345",
                    OrderData.CurrencyCode,
                    OrderData.OrderLines),
                new CreateOrderCommand(
                    OrderData.PointOfSalePhone,
                    OrderData.DistributorPhone,
                    "City2",
                    "Street2",
                    "67890",
                    OrderData.CurrencyCode,
                    OrderData.OrderLines)
            };

            var command = new BulkCreateOrdersCommand(orders);

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count);

            // Verify orders were actually created
            foreach (var orderId in result.Value)
            {
                var getOrderResult = await Sender.Send(new GetOrderQuery(orderId));
                Assert.False(getOrderResult.IsFailure);
                Assert.NotNull(getOrderResult.Value);
            }
        }

        [Fact]
        public async Task BulkCreateOrders_Fails_When_One_Order_Has_Invalid_Product()
        {
            // Arrange - Seed dependencies but use invalid product
            await OrderData.SeedDependenciesAsync(Sender);

            var validOrderLines = OrderData.OrderLines;
            var invalidOrderLines = new List<CreateOrderLineCommand>
            {
                new CreateOrderLineCommand("INVALID_PRODUCT_ID", 1)
            };

            var orders = new List<CreateOrderCommand>
            {
                new CreateOrderCommand(
                    OrderData.PointOfSalePhone,
                    OrderData.DistributorPhone,
                    "City1",
                    "Street1",
                    "12345",
                    OrderData.CurrencyCode,
                    validOrderLines),
                new CreateOrderCommand(
                    OrderData.PointOfSalePhone,
                    OrderData.DistributorPhone,
                    "City2",
                    "Street2",
                    "67890",
                    OrderData.CurrencyCode,
                    invalidOrderLines) // This should cause the whole operation to fail
            };

            var command = new BulkCreateOrdersCommand(orders);

            // Act
            var result = await Sender.Send(command);

            // Assert - The entire operation should fail
            Assert.True(result.IsFailure);
            Assert.Contains("Product", result.Error.Code);
        }

        [Fact]
        public async Task BulkCreateOrders_Fails_When_Point_Of_Sale_Not_Found()
        {
            // Arrange - Seed dependencies but use invalid POS
            await OrderData.SeedDependenciesAsync(Sender);

            var orders = new List<CreateOrderCommand>
            {
                new CreateOrderCommand(
                    "+59899999999", // Invalid POS phone
                    OrderData.DistributorPhone,
                    "City1",
                    "Street1",
                    "12345",
                    OrderData.CurrencyCode,
                    OrderData.OrderLines)
            };

            var command = new BulkCreateOrdersCommand(orders);

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("PointOfSale", result.Error.Code);
        }

        [Fact]
        public async Task BulkCreateOrders_Fails_When_Distributor_Not_Found()
        {
            // Arrange - Seed dependencies but use invalid distributor
            await OrderData.SeedDependenciesAsync(Sender);

            var orders = new List<CreateOrderCommand>
            {
                new CreateOrderCommand(
                    OrderData.PointOfSalePhone,
                    "+59899999998", // Invalid distributor phone
                    "City1",
                    "Street1",
                    "12345",
                    OrderData.CurrencyCode,
                    OrderData.OrderLines)
            };

            var command = new BulkCreateOrdersCommand(orders);

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Distributor", result.Error.Code);
        }
    }
}