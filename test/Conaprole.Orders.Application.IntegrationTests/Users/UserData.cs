using Conaprole.Orders.Application.Users.RegisterUser;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

/// <summary>
/// Encapsula datos y creación de un usuario para tests de integración.
/// </summary>
public static class UserData
{
    public const string Email = "test@conaprole.com";
    public const string FirstName = "Test";
    public const string LastName = "User";
    public const string Password = "Test123456!";

    // Alternative data for tests that need multiple users
    public const string AlternativeEmail = "test2@conaprole.com";

    /// <summary>
    /// Comando preconfigurado para crear el usuario.
    /// </summary>
    public static RegisterUserCommand CreateCommand =>
        new(
            Email,
            FirstName,
            LastName,
            Password
        );

    /// <summary>
    /// Comando preconfigurado para crear el usuario con distribuidor.
    /// </summary>
    public static RegisterUserCommand CreateCommandWithDistributor(string distributorPhoneNumber) =>
        new(
            Email,
            FirstName,
            LastName,
            Password,
            distributorPhoneNumber
        );

    /// <summary>
    /// Comando preconfigurado para crear un usuario alternativo.
    /// </summary>
    public static RegisterUserCommand CreateAlternativeCommand =>
        new(
            AlternativeEmail,
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
}