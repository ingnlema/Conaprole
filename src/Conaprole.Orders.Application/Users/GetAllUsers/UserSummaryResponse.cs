namespace Conaprole.Orders.Application.Users.GetAllUsers;

public sealed class UserSummaryResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public DateTime CreatedOnUtc { get; init; }
}