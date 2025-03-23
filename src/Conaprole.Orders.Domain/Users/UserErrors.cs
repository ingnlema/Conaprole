using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Users;

public static class UserErrors
{
    public static Error NotFound = new(
        "User.Found",
        "The user with the specified identifier was not found");

    public static Error InvalidCredentials = new(
        "User.InvalidCredentials",
        "The provided credentials were invalid");
    
    public static Error IdentityMissing = new(
        "User.IdentityMissing",
        "No identity identifier was found in the current user context");
}