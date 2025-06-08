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
        public const string CurrencyCode = "UYU";
        public const int OrderLineQuantity = 2;

        /// <summary>
        /// Comando preconfigurado para crear la orden con dependencias.
        /// </summary>
        public static CreateOrderCommand CreateCommand =>
            new(
                PointOfSaleData.PhoneNumber,
                DistributorData.PhoneNumber,
                PointOfSaleData.City,
                PointOfSaleData.Street,
                PointOfSaleData.ZipCode,
                CurrencyCode,
                new List<CreateOrderLineCommand>
                {
                    new(ProductData.ExternalProductId, OrderLineQuantity)
                }
            );

        /// <summary>
        /// Crea todas las dependencias necesarias y luego la orden, devuelve el ID de la orden.
        /// </summary>
        public static async Task<Guid> SeedAsync(ISender sender)
        {
            // 1. Crear el producto
            await ProductData.SeedAsync(sender);

            // 2. Crear el distribuidor
            await DistributorData.SeedAsync(sender);

            // 3. Crear el punto de venta
            await PointOfSaleData.SeedAsync(sender);

            // 4. Crear la orden
            var result = await sender.Send(CreateCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding order: {result.Error.Code}");
            
            return result.Value;
        }
    }
}