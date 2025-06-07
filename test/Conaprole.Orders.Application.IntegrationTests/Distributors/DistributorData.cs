using Conaprole.Orders.Application.Distributors.CreateDistributor;
using MediatR;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    /// <summary>
    /// Encapsula datos y creación de un distribuidor para tests de integración.
    /// </summary>
    public static class DistributorData
    {
        public const string Name = "Distribuidor Test";
        public const string Address = "Calle Falsa 123";
        public static readonly List<Category> DefaultCategories = new() { Category.LACTEOS };

        /// <summary>
        /// Crea el distribuidor vía MediatR con un número de teléfono único y devuelve su ID y el número de teléfono usado.
        /// </summary>
        public static async Task<(Guid Id, string PhoneNumber)> SeedAsync(ISender sender)
        {
            // Generate a unique phone number using current timestamp
            var uniquePhoneNumber = $"+59899{DateTime.UtcNow.Ticks % 1000000:D6}";
            
            var command = new CreateDistributorCommand(
                Name,
                uniquePhoneNumber,
                Address,
                DefaultCategories
            );
            
            var result = await sender.Send(command);
            if (result.IsFailure)
                throw new Exception($"Error seeding distributor: {result.Error.Code}");
            return (result.Value, uniquePhoneNumber);
        }
    }
}