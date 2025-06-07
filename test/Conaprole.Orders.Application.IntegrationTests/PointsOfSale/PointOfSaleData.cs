using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    /// <summary>
    /// Encapsula datos y creación de un punto de venta para tests de integración.
    /// </summary>
    public static class PointOfSaleData
    {
        public const string Name = "Punto de Venta Test";
        public const string City = "Montevideo";
        public const string Street = "Avenida Test 456";
        public const string ZipCode = "11300";

        /// <summary>
        /// Crea el punto de venta vía MediatR con un número de teléfono único y devuelve su ID y el número de teléfono usado.
        /// </summary>
        public static async Task<(Guid Id, string PhoneNumber)> SeedAsync(ISender sender)
        {
            // Generate a unique phone number using current timestamp
            var uniquePhoneNumber = $"+59899{DateTime.UtcNow.Ticks % 1000000:D6}";
            
            var command = new CreatePointOfSaleCommand(
                Name,
                uniquePhoneNumber,
                City,
                Street,
                ZipCode
            );
            
            var result = await sender.Send(command);
            if (result.IsFailure)
                throw new Exception($"Error seeding point of sale: {result.Error.Code}");
            return (result.Value, uniquePhoneNumber);
        }
    }
}