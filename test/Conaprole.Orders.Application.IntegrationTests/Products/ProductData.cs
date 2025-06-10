using Conaprole.Orders.Application.Products.CreateProduct;
using MediatR;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Application.Abstractions.Data;
using Dapper;

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
        /// Crea el producto vía MediatR y devuelve su ID.
        /// Si el producto ya existe con el mismo ExternalId, devuelve el ID del existente.
        /// </summary>
        public static async Task<Guid> SeedAsync(ISender sender, ISqlConnectionFactory sqlConnectionFactory)
        {
            var result = await sender.Send(CreateCommand);
            if (result.IsFailure && result.Error.Code == "Product.DuplicatedExternalId")
            {
                // If product already exists, get it by ExternalId using direct SQL
                using var connection = sqlConnectionFactory.CreateConnection();
                const string sql = "SELECT id FROM products WHERE external_product_id = @ExternalProductId";
                var existingId = await connection.QueryFirstOrDefaultAsync<Guid?>(sql, new { ExternalProductId });
                
                if (existingId.HasValue)
                    return existingId.Value;
            }
            
            if (result.IsFailure)
                throw new Exception($"Error seeding product: {result.Error.Code}");
                
            return result.Value;
        }

        /// <summary>
        /// Datos para un segundo producto para tests que requieren múltiples productos.
        /// </summary>
        public const string SecondProductId = "SECOND_SKU";
        public const string SecondProductName = "Segundo Producto";
        public const decimal SecondProductUnitPrice = 150m;
        public const string SecondProductDescription = "Segundo producto para integration tests";

        /// <summary>
        /// Comando preconfigurado para crear el segundo producto.
        /// </summary>
        public static CreateProductCommand SecondProductCommand =>
            new(
                SecondProductId,
                SecondProductName,
                SecondProductUnitPrice,
                CurrencyCode,
                SecondProductDescription,
                DefaultCategory
            );

        /// <summary>
        /// Crea el segundo producto vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedSecondProductAsync(ISender sender)
        {
            var result = await sender.Send(SecondProductCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding second product: {result.Error.Code}");
            return result.Value;
        }
    }
}
