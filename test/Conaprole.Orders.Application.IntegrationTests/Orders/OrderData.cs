using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
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
            var pointOfSalePhoneNumber = $"+59899{Random.Shared.Next(100000, 999999)}";
            var command = new CreatePointOfSaleCommand(
                PointOfSaleData.Name,
                pointOfSalePhoneNumber,
                PointOfSaleData.City,
                PointOfSaleData.Street,
                PointOfSaleData.ZipCode
            );
            
            var pointOfSaleResult = await sender.Send(command);
            if (pointOfSaleResult.IsFailure)
                throw new Exception($"Error seeding point of sale: {pointOfSaleResult.Error.Code}");
            var pointOfSaleId = pointOfSaleResult.Value;

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
    }
}