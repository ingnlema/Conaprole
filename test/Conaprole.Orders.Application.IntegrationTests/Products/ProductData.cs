using Conaprole.Orders.Application.Products.CreateProduct;
using MediatR;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.IntegrationTests.Products

{
    /// <summary>
    /// Encapsula datos y creación de un producto para tests de integración.
    /// </summary>
    public static class ProductData
    {
        public const string ExternalProductId = "GLOBAL_SKU";
        public const string Name              = "Producto Global";
        public const decimal UnitPrice        = 100m;
        public const string CurrencyCode      = "UYU";
        public const string Description       = "Producto para integration tests";
        public const Category DefaultCategory = Category.LACTEOS;

        /// <summary>
        /// Comando preconfigurado para crear el producto.
        /// </summary>
        public static CreateProductCommand CreateCommand =>
            new(
                ExternalProductId,
                Name,
                UnitPrice,
                CurrencyCode,
                Description,
                DefaultCategory
            );

        /// <summary>
        /// Crea el producto vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedAsync(ISender sender)
        {
            var result = await sender.Send(CreateCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding product: {result.Error.Code}");
            return result.Value;
        }

        /// <summary>
        /// Crea un producto con ExternalProductId específico vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedWithExternalIdAsync(ISender sender, string externalProductId)
        {
            var command = new CreateProductCommand(
                externalProductId,
                Name,
                UnitPrice,
                CurrencyCode,
                Description,
                DefaultCategory
            );
            
            var result = await sender.Send(command);
            if (result.IsFailure)
                throw new Exception($"Error seeding product: {result.Error.Code}");
            return result.Value;
        }
    }
}
