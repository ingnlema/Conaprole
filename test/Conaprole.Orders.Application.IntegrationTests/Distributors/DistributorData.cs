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
        public const string PhoneNumber = "+59899887766";
        public const string Name = "Distribuidor Test";
        public const string Address = "Calle Falsa 123";
        public static readonly List<Category> DefaultCategories = new() { Category.LACTEOS };

        /// <summary>
        /// Comando preconfigurado para crear el distribuidor.
        /// </summary>
        public static CreateDistributorCommand CreateCommand =>
            new(
                Name,
                PhoneNumber,
                Address,
                DefaultCategories
            );

        /// <summary>
        /// Crea un comando de distribuidor con datos únicos.
        /// </summary>
        public static CreateDistributorCommand CreateUniqueCommand(string uniqueIdentifier) =>
            new(
                $"{Name} {uniqueIdentifier}",
                $"+59899887{uniqueIdentifier.PadLeft(3, '0')}",
                $"{Address} {uniqueIdentifier}",
                DefaultCategories
            );

        /// <summary>
        /// Crea el distribuidor vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedAsync(ISender sender)
        {
            var result = await sender.Send(CreateCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding distributor: {result.Error.Code}");
            return result.Value;
        }

        /// <summary>
        /// Crea un distribuidor único vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<(Guid Id, string PhoneNumber)> SeedUniqueAsync(ISender sender, string uniqueIdentifier)
        {
            var command = CreateUniqueCommand(uniqueIdentifier);
            var result = await sender.Send(command);
            if (result.IsFailure)
                throw new Exception($"Error seeding distributor: {result.Error.Code}");
            return (result.Value, command.PhoneNumber);
        }
    }
}