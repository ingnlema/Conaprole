using Conaprole.Orders.Application.Users.RegisterUser;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

/// <summary>
/// Encapsula datos y creación de un usuario para tests de integración.
/// </summary>
public static class UserData
{
    public const string Email = "logintest@test.com";
    public const string FirstName = "Login";
    public const string LastName = "Test";
    public const string Password = "testpassword123";

    /// <summary>
    /// Comando preconfigurado para registrar el usuario.
    /// </summary>
    public static RegisterUserCommand RegisterCommand =>
        new(
            Email,
            FirstName,
            LastName,
            Password
        );

    /// <summary>
    /// Registra el usuario vía MediatR y devuelve su ID.
    /// </summary>
    public static async Task<Guid> SeedAsync(ISender sender)
    {
        var result = await sender.Send(RegisterCommand);
        if (result.IsFailure)
            throw new Exception($"Error seeding user: {result.Error.Code}");
        return result.Value;
    }
}