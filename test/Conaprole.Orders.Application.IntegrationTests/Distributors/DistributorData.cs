using Conaprole.Orders.Application.Distributors.CreateDistributor;
using MediatR;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Application.Abstractions.Data;
using Dapper;

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
        /// Crea el distribuidor vía MediatR y devuelve su ID.
        /// Si el distribuidor ya existe con el mismo PhoneNumber, devuelve el ID del existente.
        /// </summary>
        public static async Task<Guid> SeedAsync(ISender sender, ISqlConnectionFactory sqlConnectionFactory)
        {
            var result = await sender.Send(CreateCommand);
            if (result.IsFailure && result.Error.Code == "Distributor.AlreadyExists")
            {
                // If distributor already exists, get it by PhoneNumber using direct SQL
                using var connection = sqlConnectionFactory.CreateConnection();
                const string sql = "SELECT id FROM distributor WHERE phone_number = @PhoneNumber";
                var existingId = await connection.QueryFirstOrDefaultAsync<Guid?>(sql, new { PhoneNumber });
                
                if (existingId.HasValue)
                    return existingId.Value;
            }
            
            if (result.IsFailure)
                throw new Exception($"Error seeding distributor: {result.Error.Code}");
                
            return result.Value;
        }
    }
}