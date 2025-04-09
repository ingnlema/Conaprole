using Conaprole.Orders.Api.Controllers.Users;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

public static class UserData
{
    public static RegisterUserRequest RegisterTestUserRequest = new ("test@test.com","test","test", "12345");
}