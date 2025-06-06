using Conaprole.Orders.Application.Users.RegisterUser;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Users
{
    /// <summary>
    /// Encapsula datos y creación de un usuario para tests de integración.
    /// </summary>
    public static class UserData
    {
        public const string Email = "integration.test@test.com";
        public const string FirstName = "Test";
        public const string LastName = "User";
        public const string Password = "Test123!";
        public const string? DistributorPhoneNumber = null;

        /// <summary>
        /// Comando preconfigurado para crear el usuario.
        /// </summary>
        public static RegisterUserCommand CreateCommand =>
            new(
                Email,
                FirstName,
                LastName,
                Password,
                DistributorPhoneNumber
            );

        /// <summary>
        /// Crea el usuario vía MediatR y devuelve su ID.
        /// </summary>
        public static async Task<Guid> SeedAsync(ISender sender)
        {
            var result = await sender.Send(CreateCommand);
            if (result.IsFailure)
                throw new Exception($"Error seeding user: {result.Error.Code}");
            return result.Value;
        }
    }
}