using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    public record OrderDataResult(Guid OrderId, string ProductExternalId, string DistributorPhoneNumber, string PointOfSalePhoneNumber);

    /// <summary>
    /// Encapsula datos y creación de una orden para tests de integración.
    /// </summary>
    public static class OrderData
    {
        public const string City = "Montevideo";
        public const string Street = "Test Street";
        public const string ZipCode = "11200";
        public const string CurrencyCode = "UYU";

        /// <summary>
        /// Crea una orden completa con todas las dependencias necesarias.
        /// </summary>
        public static async Task<OrderDataResult> SeedAsync(ISender sender)
        {
            // 1. Crear producto único
            var uniqueExternalId = $"PRODUCT_{Guid.NewGuid():N}";
            var productId = await ProductData.SeedWithExternalIdAsync(sender, uniqueExternalId);

            // 2. Crear distribuidor único
            var distributorPhoneNumber = $"+59899{Random.Shared.Next(100000, 999999)}";
            var distributorId = await DistributorData.SeedWithPhoneAsync(sender, distributorPhoneNumber);

            // 3. Crear punto de venta único
            var pointOfSalePhoneNumber = $"+59891{Random.Shared.Next(100000, 999999)}";
            var pointOfSaleId = await PointOfSaleData.SeedWithPhoneAsync(sender, pointOfSalePhoneNumber);

            // 4. Crear orden con línea de producto
            var orderLine = new CreateOrderLineCommand(uniqueExternalId, 1);
            var createOrderCommand = new CreateOrderCommand(
                pointOfSalePhoneNumber,
                distributorPhoneNumber,
                City,
                Street,
                ZipCode,
                CurrencyCode,
                new List<CreateOrderLineCommand> { orderLine }
            );

            var result = await sender.Send(createOrderCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding order: {result.Error.Code}");
            
            return new OrderDataResult(result.Value, uniqueExternalId, distributorPhoneNumber, pointOfSalePhoneNumber);
        }

        /// <summary>
        /// Crea una orden sin líneas de productos.
        /// </summary>
        public static async Task<Guid> SeedEmptyOrderAsync(ISender sender)
        {
            // 1. Crear distribuidor único
            var distributorPhoneNumber = $"+59899{Random.Shared.Next(100000, 999999)}";
            var distributorId = await DistributorData.SeedWithPhoneAsync(sender, distributorPhoneNumber);

            // 2. Crear punto de venta único
            var pointOfSalePhoneNumber = $"+59891{Random.Shared.Next(100000, 999999)}";
            var pointOfSaleId = await PointOfSaleData.SeedWithPhoneAsync(sender, pointOfSalePhoneNumber);

            // 3. Crear orden sin líneas de producto
            var createOrderCommand = new CreateOrderCommand(
                pointOfSalePhoneNumber,
                distributorPhoneNumber,
                City,
                Street,
                ZipCode,
                CurrencyCode,
                new List<CreateOrderLineCommand>()
            );

            var result = await sender.Send(createOrderCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding empty order: {result.Error.Code}");
            
            return result.Value;
        }
    }
}