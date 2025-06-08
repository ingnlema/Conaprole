using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
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
        public static async Task<Guid> SeedAsync(ISender sender)
        {
            // 1. Crear producto
            var productId = await ProductData.SeedAsync(sender);

            // 2. Crear distribuidor
            var distributorId = await DistributorData.SeedAsync(sender);

            // 3. Crear punto de venta
            var pointOfSaleId = await PointOfSaleData.SeedAsync(sender);

            // 4. Crear orden con línea de producto
            var orderLine = new CreateOrderLineCommand(ProductData.ExternalProductId, 1);
            var createOrderCommand = new CreateOrderCommand(
                PointOfSaleData.PhoneNumber,
                DistributorData.PhoneNumber,
                City,
                Street,
                ZipCode,
                CurrencyCode,
                new List<CreateOrderLineCommand> { orderLine }
            );

            var result = await sender.Send(createOrderCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding order: {result.Error.Code}");
            
            return result.Value;
        }

        /// <summary>
        /// Crea una orden sin líneas de productos.
        /// </summary>
        public static async Task<Guid> SeedEmptyOrderAsync(ISender sender)
        {
            // 1. Crear distribuidor
            var distributorId = await DistributorData.SeedAsync(sender);

            // 2. Crear punto de venta
            var pointOfSaleId = await PointOfSaleData.SeedAsync(sender);

            // 3. Crear orden sin líneas de producto
            var createOrderCommand = new CreateOrderCommand(
                PointOfSaleData.PhoneNumber,
                DistributorData.PhoneNumber,
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