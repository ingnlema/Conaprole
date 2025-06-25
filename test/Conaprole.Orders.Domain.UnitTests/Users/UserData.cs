using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Domain.UnitTests.Users;

internal static class UserData
{
    public static readonly FirstName FirstName = new ("First");
    public static readonly LastName LastName = new ("Last");
    public static readonly Email Email = new ("test@test.com");
    public static readonly DateTime CreatedAt = DateTime.UtcNow;
}