using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Domain.Shared;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    /// <summary>
    /// Encapsula datos y creación de un punto de venta para tests de integración.
    /// </summary>
    public static class PointOfSaleData
    {
        public const string Name = "Punto de Venta Test";
        public const string PhoneNumber = "+59891234567";
        public const string City = "Montevideo";
        public const string Street = "Calle Test 123";
        public const string ZipCode = "11200";

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