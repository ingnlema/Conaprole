namespace Conaprole.Orders.Api.Controllers.Users;

/// <summary>
/// Request model for user registration
/// </summary>
/// <param name="Email">User's email address</param>
/// <param name="FirstName">User's first name</param>
/// <param name="LastName">User's last name</param>
/// <param name="Password">User's password</param>
/// <param name="DistributorPhoneNumber">Optional distributor phone number for distributor users</param>
public sealed record RegisterUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? DistributorPhoneNumber = null);