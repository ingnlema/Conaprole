using Conaprole.Orders.Application.Users.RegisterUser;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

/// <summary>
/// Encapsula datos y creación de un usuario para tests de integración.
/// </summary>
public static class UserData
{
    public const string FirstName = "Test";
    public const string LastName = "User";
    public const string Password = "Test123456!";

    /// <summary>
    /// Genera un email único para evitar conflictos en Keycloak.
    /// </summary>
    public static string GenerateUniqueEmail() => $"test-{Guid.NewGuid()}@conaprole.com";

    /// <summary>
    /// Genera un email alternativo único para tests que necesitan múltiples usuarios.
    /// </summary>
    public static string GenerateUniqueAlternativeEmail() => $"test2-{Guid.NewGuid()}@conaprole.com";

    /// <summary>
    /// Comando para crear un usuario con email único.
    /// </summary>
    public static RegisterUserCommand CreateCommand =>
        new(
            GenerateUniqueEmail(),
            FirstName,
            LastName,
            Password
        );

    /// <summary>
    /// Comando para crear un usuario con distribuidor con email único.
    /// </summary>
    public static RegisterUserCommand CreateCommandWithDistributor(string distributorPhoneNumber) =>
        new(
            GenerateUniqueEmail(),
            FirstName,
            LastName,
            Password,
            distributorPhoneNumber
        );

    /// <summary>
    /// Comando para crear un usuario alternativo con email único.
    /// </summary>
    public static RegisterUserCommand CreateAlternativeCommand =>
        new(
            GenerateUniqueAlternativeEmail(),
            FirstName,
            LastName,
            Password
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

    /// <summary>
    /// Crea el usuario con distribuidor vía MediatR y devuelve su ID.
    /// </summary>
    public static async Task<Guid> SeedWithDistributorAsync(ISender sender, string distributorPhoneNumber)
    {
        var result = await sender.Send(CreateCommandWithDistributor(distributorPhoneNumber));
        if (result.IsFailure)
            throw new Exception($"Error seeding user with distributor: {result.Error.Code}");
        return result.Value;
    }

    /// <summary>
    /// Crea un usuario específico con email conocido y devuelve el email y ID.
    /// Útil para tests que necesitan verificar el email específico usado.
    /// </summary>
    public static async Task<(string email, Guid userId)> SeedWithKnownEmailAsync(ISender sender)
    {
        var email = GenerateUniqueEmail();
        var command = new RegisterUserCommand(email, FirstName, LastName, Password);
        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new Exception($"Error seeding user: {result.Error.Code}");
        return (email, result.Value);
    }

    /// <summary>
    /// Crea un usuario con distribuidor específico con email conocido y devuelve el email y ID.
    /// </summary>
    public static async Task<(string email, Guid userId)> SeedWithDistributorAndKnownEmailAsync(ISender sender, string distributorPhoneNumber)
    {
        var email = GenerateUniqueEmail();
        var command = new RegisterUserCommand(email, FirstName, LastName, Password, distributorPhoneNumber);
        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new Exception($"Error seeding user with distributor: {result.Error.Code}");
        return (email, result.Value);
    }
}