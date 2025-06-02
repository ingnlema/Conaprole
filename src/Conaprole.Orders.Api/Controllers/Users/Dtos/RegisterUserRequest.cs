namespace Conaprole.Orders.Api.Controllers.Users;

public sealed record RegisterUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? DistributorPhoneNumber = null);