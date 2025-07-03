namespace Conaprole.Orders.Api.Controllers.Users;

/// <summary>
/// Request model for user authentication/login
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="Password">User's password</param>
public sealed record LogInUserRequest(string Email, string Password);