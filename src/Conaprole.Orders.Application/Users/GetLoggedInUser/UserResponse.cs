namespace Conaprole.Orders.Application.Users.GetLoggedInUser;

public sealed class UserResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public List<string> Roles { get; init; } = new();

    public string? UserType { get; init; }

    public Guid? DistributorId { get; init; }

    public string? DistributorPhoneNumber { get; init; }
}