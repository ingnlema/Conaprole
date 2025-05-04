using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.Controllers.Products;

namespace Conaprole.Orders.Api.FunctionalTests.Products
{
    public static class ProductData
    {
        public const string ExternalProductId = "GLOBAL_SKU";
        public const string Name              = "Producto Global";
        public const decimal UnitPrice        = 50m;
        public const string CurrencyCode      = "UYU";
        public const string Description       = "Producto para todos los tests funcionales";
        public static readonly List<string> Categories = new() { "Global" };

        public static CreateProductRequest CreateRequest =>
            new(
                ExternalProductId,
                Name,
                UnitPrice,
                CurrencyCode,
                Description,
                new List<string>(Categories)
            );

        public static async Task<Guid> CreateAsync(HttpClient client) =>
            await CreateAsync(client, ExternalProductId);

        public static async Task<Guid> CreateAsync(HttpClient client, string externalProductId)
        {
            var req = new CreateProductRequest(
                externalProductId,
                Name,
                UnitPrice,
                CurrencyCode,
                Description,
                new List<string>(Categories)
            );
            var response = await client.PostAsJsonAsync("api/Products", req);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        public static OrderLineRequest OrderLine(int quantity = 1) =>
            new(ExternalProductId, quantity);

        public static OrderLineRequest OrderLine(string externalProductId, int quantity) =>
            new(externalProductId, quantity);
    }
}