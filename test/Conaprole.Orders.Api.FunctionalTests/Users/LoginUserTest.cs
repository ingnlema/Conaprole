using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Users;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;
[Collection("ApiCollection")]
public class LoginUserTest : BaseFunctionalTest
{
    private const string Email = "login@test.com";
    private const string Password = "12345";
    
    public LoginUserTest(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_ShouldReturnUnathorized_WhenUserDoesNotExists()
    {
        var request = new LogInUserRequest(Email, Password);
        
        var response = await HttpClient.PostAsJsonAsync("/api/users/login", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    }
    
    [Fact]
    public async Task Login_ShouldReturnOk_WhenUserExists()
    {
        var registerRequest = new RegisterUserRequest(Email, "first", "last", Password);
    

        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
    

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        Debug.WriteLine("Register Response Content: " + registerContent);

        registerContent.Should().NotBeNullOrEmpty("porque se espera que el registro devuelva información del usuario creado");
    
        var loginRequest = new LogInUserRequest(Email, Password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
    
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        Debug.WriteLine("Login Response Content: " + loginContent);
    }

}