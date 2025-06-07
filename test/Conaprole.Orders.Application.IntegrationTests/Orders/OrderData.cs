using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    /// <summary>
    /// Encapsula datos y creación de órdenes para tests de integración.
    /// </summary>
    public static class OrderData
    {
        public const string City = "Montevideo";
        public const string Street = "Avenida Principal 456";
        public const string ZipCode = "11300";
        public const string CurrencyCode = "UYU";

        /// <summary>
        /// Crea una orden con dos líneas de productos diferentes vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedOrderWithTwoLinesAsync(ISender sender)
        {
            // Generar IDs únicos para evitar conflictos (usar solo 4 dígitos)
            var uniqueId = Guid.NewGuid().ToString("N")[..4];
            var firstProductExternalId = $"{ProductData.ExternalProductId}_{uniqueId}";
            var secondProductExternalId = $"{SecondProductExternalId}_{uniqueId}";
            var distributorPhoneNumber = $"+598998877{uniqueId}"; // Max 20 chars
            var pointOfSalePhoneNumber = $"+598912345{uniqueId}"; // Max 20 chars

            // Crear los datos necesarios
            var productId1 = await SeedUniqueProductAsync(sender, firstProductExternalId, ProductData.Name, ProductData.UnitPrice, ProductData.Description);
            var productId2 = await SeedUniqueProductAsync(sender, secondProductExternalId, SecondProductName, SecondProductUnitPrice, SecondProductDescription);
            var distributorId = await SeedUniqueDistributorAsync(sender, distributorPhoneNumber);
            var pointOfSaleId = await SeedUniquePointOfSaleAsync(sender, pointOfSalePhoneNumber);

            // Crear las líneas de la orden
            var orderLines = new List<CreateOrderLineCommand>
            {
                new(firstProductExternalId, 2),
                new(secondProductExternalId, 1)
            };

            // Crear el comando para la orden
            var createOrderCommand = new CreateOrderCommand(
                pointOfSalePhoneNumber,
                distributorPhoneNumber,
                City,
                Street,
                ZipCode,
                CurrencyCode,
                orderLines
            );

            var result = await sender.Send(createOrderCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding order: {result.Error.Code}");
            return result.Value;
        }

        /// <summary>
        /// Crea una orden con una sola línea de producto vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedOrderWithSingleLineAsync(ISender sender)
        {
            // Generar IDs únicos para evitar conflictos (usar solo 4 dígitos)
            var uniqueId = Guid.NewGuid().ToString("N")[..4];
            var productExternalId = $"{ProductData.ExternalProductId}_{uniqueId}";
            var distributorPhoneNumber = $"+598998877{uniqueId}"; // Max 20 chars
            var pointOfSalePhoneNumber = $"+598912345{uniqueId}"; // Max 20 chars

            // Crear los datos necesarios
            var productId = await SeedUniqueProductAsync(sender, productExternalId, ProductData.Name, ProductData.UnitPrice, ProductData.Description);
            var distributorId = await SeedUniqueDistributorAsync(sender, distributorPhoneNumber);
            var pointOfSaleId = await SeedUniquePointOfSaleAsync(sender, pointOfSalePhoneNumber);

            // Crear la línea de la orden
            var orderLines = new List<CreateOrderLineCommand>
            {
                new(productExternalId, 1)
            };

            // Crear el comando para la orden
            var createOrderCommand = new CreateOrderCommand(
                pointOfSalePhoneNumber,
                distributorPhoneNumber,
                City,
                Street,
                ZipCode,
                CurrencyCode,
                orderLines
            );

            var result = await sender.Send(createOrderCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding single line order: {result.Error.Code}");
            return result.Value;
        }

        // Constantes para el segundo producto
        public const string SecondProductExternalId = "GLOBAL_SKU_2";
        public const string SecondProductName = "Producto Global 2";
        public const decimal SecondProductUnitPrice = 150m;
        public const string SecondProductDescription = "Segundo producto para integration tests";

        /// <summary>
        /// Crea un segundo producto diferente para tener múltiples líneas en las órdenes.
        /// </summary>
        private static async Task<Guid> SeedSecondProductAsync(ISender sender)
        {
            var createProductCommand = new Conaprole.Orders.Application.Products.CreateProduct.CreateProductCommand(
                SecondProductExternalId,
                SecondProductName,
                SecondProductUnitPrice,
                CurrencyCode,
                SecondProductDescription,
                ProductData.DefaultCategory
            );

            var result = await sender.Send(createProductCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding second product: {result.Error.Code}");
            return result.Value;
        }

        /// <summary>
        /// Crea un producto con external ID único.
        /// </summary>
        private static async Task<Guid> SeedUniqueProductAsync(ISender sender, string externalId, string name, decimal unitPrice, string description)
        {
            var createProductCommand = new Conaprole.Orders.Application.Products.CreateProduct.CreateProductCommand(
                externalId,
                name,
                unitPrice,
                CurrencyCode,
                description,
                ProductData.DefaultCategory
            );

            var result = await sender.Send(createProductCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding unique product: {result.Error.Code}");
            return result.Value;
        }

        /// <summary>
        /// Crea un distribuidor con número de teléfono único.
        /// </summary>
        private static async Task<Guid> SeedUniqueDistributorAsync(ISender sender, string phoneNumber)
        {
            var createDistributorCommand = new Conaprole.Orders.Application.Distributors.CreateDistributor.CreateDistributorCommand(
                DistributorData.Name,
                phoneNumber,
                DistributorData.Address,
                DistributorData.DefaultCategories
            );

            var result = await sender.Send(createDistributorCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding unique distributor: {result.Error.Code}");
            return result.Value;
        }

        /// <summary>
        /// Crea un punto de venta con número de teléfono único.
        /// </summary>
        private static async Task<Guid> SeedUniquePointOfSaleAsync(ISender sender, string phoneNumber)
        {
            var createPointOfSaleCommand = new Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale.CreatePointOfSaleCommand(
                PointOfSaleData.Name,
                phoneNumber,
                PointOfSaleData.City,
                PointOfSaleData.Street,
                PointOfSaleData.ZipCode
            );

            var result = await sender.Send(createPointOfSaleCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding unique point of sale: {result.Error.Code}");
            return result.Value;
        }
    }
}