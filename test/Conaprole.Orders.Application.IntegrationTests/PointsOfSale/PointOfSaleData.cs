using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    /// <summary>
    /// Encapsula datos y creación de puntos de venta para tests de integración.
    /// </summary>
    public static class PointOfSaleData
    {
        public const string Name = "Punto de Venta Test";
        public const string PhoneNumber = "+59891234567";
        public const string City = "Montevideo";
        public const string Street = "Test Street 123";
        public const string ZipCode = "11200";

        public const string InactiveName = "Punto de Venta Inactivo";
        public const string InactivePhoneNumber = "+59891234568";
        public const string InactiveCity = "Montevideo";
        public const string InactiveStreet = "Inactive Street 456";
        public const string InactiveZipCode = "11300";

        /// <summary>
        /// Comando preconfigurado para crear el punto de venta activo.
        /// </summary>
        public static CreatePointOfSaleCommand CreateActiveCommand =>
            new(Name, PhoneNumber, City, Street, ZipCode);

        /// <summary>
        /// Comando preconfigurado para crear el punto de venta inactivo.
        /// </summary>
        public static CreatePointOfSaleCommand CreateInactiveCommand =>
            new(InactiveName, InactivePhoneNumber, InactiveCity, InactiveStreet, InactiveZipCode);

        /// <summary>
        /// Crea un punto de venta activo vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedActiveAsync(ISender sender)
        {
            var result = await sender.Send(CreateActiveCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding active point of sale: {result.Error.Code}");
            return result.Value;
        }

        /// <summary>
        /// Crea un punto de venta inactivo vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedInactiveAsync(ISender sender)
        {
            // Primero crear el punto de venta
            var createResult = await sender.Send(CreateInactiveCommand);
            if (createResult.IsFailure)
                throw new Exception($"Error creating point of sale for deactivation: {createResult.Error.Code}");

            // Luego desactivarlo
            var disableResult = await sender.Send(new DisablePointOfSaleCommand(InactivePhoneNumber));
            if (disableResult.IsFailure)
                throw new Exception($"Error disabling point of sale: {disableResult.Error.Code}");

            return createResult.Value;
        }
    }
}