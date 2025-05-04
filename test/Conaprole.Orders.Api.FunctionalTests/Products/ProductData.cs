
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.Controllers.Products;

namespace Conaprole.Orders.Api.FunctionalTests.Products
{
    /// <summary>
    /// Helper para test de productos: datos + creación vía API.
    /// </summary>
    public static class ProductData
    {
        public const string ExternalProductId = "GLOBAL_SKU";
        public const string Name              = "Producto Global";
        public const decimal UnitPrice        = 50m;
        public const string CurrencyCode      = "UYU";
        public const string Description       = "Producto para todos los tests funcionales";
        public static readonly List<string> Categories = new() { "Global" };

        /// <summary>
        /// Request preconfigurada para crear el producto global.
        /// </summary>
        public static CreateProductRequest CreateRequest =>
            new CreateProductRequest(
                ExternalProductId,
                Name,
                UnitPrice,
                CurrencyCode,
                Description,
                new List<string>(Categories)
            );

        /// <summary>
        /// Public helper que crea el producto a través de la API y devuelve su Id.
        /// </summary>
        public static async Task<Guid> CreateAsync(HttpClient client)
        {
            var resp = await client.PostAsJsonAsync("api/Products", CreateRequest);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<Guid>();
        }

        /// <summary>
        /// Convenience: genera una línea de pedido apuntando a este producto.
        /// </summary>
        public static OrderLineRequest OrderLine(int quantity = 1) =>
            new OrderLineRequest(ExternalProductId, quantity);
    }
}