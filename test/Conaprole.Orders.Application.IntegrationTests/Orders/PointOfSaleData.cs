using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    /// <summary>
    /// Encapsula datos y creación de un punto de venta para tests de integración.
    /// </summary>
    public static class PointOfSaleData
    {
        public const string Name = "Punto de Venta Test";
        public const string PhoneNumber = "+59898765432";
        public const string City = "Montevideo";
        public const string Street = "Avenida Test 456";
        public const string ZipCode = "11100";

        /// <summary>
        /// Comando preconfigurado para crear el punto de venta.
        /// </summary>
        public static CreatePointOfSaleCommand CreateCommand =>
            new(
                Name,
                PhoneNumber,
                City,
                Street,
                ZipCode
            );

        /// <summary>
        /// Crea el punto de venta vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedAsync(ISender sender)
        {
            var result = await sender.Send(CreateCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding point of sale: {result.Error.Code}");
            return result.Value;
        }
    }
}