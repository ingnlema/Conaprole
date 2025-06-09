using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    /// <summary>
    /// Encapsula datos y creación de una orden para tests de integración.
    /// </summary>
    public static class OrderData
    {
        public const string PointOfSalePhone = "+59891234567";
        public const string DistributorPhone = "+59899887766";
        public const string DeliveryCity = "Montevideo";
        public const string DeliveryStreet = "Calle Test 123";
        public const string DeliveryZipCode = "11000";
        public const string CurrencyCode = "UYU";
        public const int OrderLineQuantity = 2;

        public static List<CreateOrderLineCommand> OrderLines =>
            new List<CreateOrderLineCommand>
            {
                new(ProductData.ExternalProductId, OrderLineQuantity)
            };

        /// <summary>
        /// Crea una orden completa con todas las dependencias necesarias.
        /// </summary>
        public static async Task<Guid> SeedAsync(ISender sender)
        {
            // 1) Crear producto
            var productId = await ProductData.SeedAsync(sender);

            // 2) Crear distribuidor
            var distributorId = await DistributorData.SeedAsync(sender);

            // 3) Crear punto de venta
            var pointOfSaleId = await SeedPointOfSaleAsync(sender);

            // 4) Crear orden
            var createOrderCommand = new CreateOrderCommand(
                PointOfSalePhone,
                DistributorPhone,
                DeliveryCity,
                DeliveryStreet,
                DeliveryZipCode,
                CurrencyCode,
                OrderLines
            );

            var result = await sender.Send(createOrderCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding order: {result.Error.Code}");

            return result.Value;
        }

        /// <summary>
        /// Crea una orden con múltiples líneas para tests de eliminación.
        /// </summary>
        public static async Task<Guid> SeedOrderWithMultipleLinesAsync(ISender sender)
        {
            // 1) Crear productos
            var firstProductId = await ProductData.SeedAsync(sender);
            var secondProductId = await ProductData.SeedSecondProductAsync(sender);

            // 2) Crear distribuidor
            var distributorId = await DistributorData.SeedAsync(sender);

            // 3) Crear punto de venta
            var pointOfSaleId = await SeedPointOfSaleAsync(sender);

            // 4) Crear orden con múltiples líneas
            var createOrderCommand = new CreateOrderCommand(
                PointOfSalePhone,
                DistributorPhone,
                DeliveryCity,
                DeliveryStreet,
                DeliveryZipCode,
                CurrencyCode,
                new List<CreateOrderLineCommand>
                {
                    new(ProductData.ExternalProductId, OrderLineQuantity),
                    new(ProductData.SecondProductId, OrderLineQuantity + 1)
                }
            );

            var result = await sender.Send(createOrderCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding order with multiple lines: {result.Error.Code}");

            return result.Value;
        }

        /// <summary>
        /// Crea un punto de venta usando valores predeterminados.
        /// </summary>
        private static async Task<Guid> SeedPointOfSaleAsync(ISender sender)
        {
            var command = new CreatePointOfSaleCommand(
                "Punto de Venta Test",
                PointOfSalePhone,
                DeliveryCity,
                DeliveryStreet,
                DeliveryZipCode
            );

            var result = await sender.Send(command);
            if (result.IsFailure)
                throw new Exception($"Error seeding point of sale: {result.Error.Code}");

            return result.Value;
        }

        /// <summary>
        /// Seeds all necessary dependencies (product, distributor, point of sale) without creating an order.
        /// </summary>
        public static async Task SeedDependenciesAsync(ISender sender)
        {
            // 1) Create product
            await ProductData.SeedAsync(sender);
            
            // 2) Create distributor
            await DistributorData.SeedAsync(sender);
            
            // 3) Create point of sale
            await SeedPointOfSaleAsync(sender);
        }
    }
}