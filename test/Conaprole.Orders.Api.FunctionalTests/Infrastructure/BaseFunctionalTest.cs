using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Users;
using Conaprole.Orders.Application.Users.LoginUser;

namespace Conaprole.Orders.Api.FunctionalTests.Infrastructure;

public abstract class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient;

    protected BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
    }

    protected async Task<string> GetAccessToken()
    {
        HttpResponseMessage loginResponse = await HttpClient.PostAsJsonAsync(
            "/api/users/login",
            new LogInUserRequest(
                UserData.RegisterTestUserRequest.Email,
                UserData.RegisterTestUserRequest.Password));
        
        var accessTokenResponse = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

        return accessTokenResponse!.AccessToken;
    }
}