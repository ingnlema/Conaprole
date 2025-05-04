using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;
[Collection("ApiCollection")]
public class RegisterUserTest : BaseFunctionalTest
{
    public RegisterUserTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        var request = new RegisterUserRequest("create@test.com","first","last", "12345");
        
        var response = await HttpClient.PostAsJsonAsync("/api/users/register", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

}