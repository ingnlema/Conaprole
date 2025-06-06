using Conaprole.Orders.Application.Users.RegisterUser;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

/// <summary>
/// Encapsula datos y creación de un usuario para tests de integración.
/// </summary>
public static class UserData
{
    public static string Email => $"logintest-{Guid.NewGuid()}@test.com";
    public const string FirstName = "Login";
    public const string LastName = "Test";
    public const string Password = "testpassword123";

    /// <summary>
    /// Comando preconfigurado para registrar el usuario.
    /// </summary>
    public static RegisterUserCommand CreateRegisterCommand() =>
        new(
            Email,
            FirstName,
            LastName,
            Password
        );

    /// <summary>
    /// Registra el usuario vía MediatR y devuelve su ID y email para usar en tests.
    /// </summary>
    public static async Task<(Guid UserId, string Email)> SeedAsync(ISender sender)
    {
        var command = CreateRegisterCommand();
        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new Exception($"Error seeding user: {result.Error.Code}");
        return (result.Value, command.Email);
    }
}