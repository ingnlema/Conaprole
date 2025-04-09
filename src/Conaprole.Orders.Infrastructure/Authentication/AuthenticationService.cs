using System.Net.Http.Json;
using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Domain.Users;
using Conaprole.Orders.Infrastructure.Authentication.Models;

namespace Conaprole.Orders.Infrastructure.Authentication;

internal sealed class AuthenticationService : IAuthenticationService
{
    private const string PasswordCredentialType = "password";

    private readonly HttpClient _httpClient;

    public AuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> RegisterAsync(
        User user,
        string password,
        CancellationToken cancellationToken = default)
    {
        var userRepresentationModel = UserRepresentationModel.FromUser(user);

        userRepresentationModel.Credentials = new CredentialRepresentationModel[]
        {
            new()
            {
                Value = password,
                Temporary = false,
                Type = PasswordCredentialType
            }
        };
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "users",
                userRepresentationModel,
                cancellationToken);

            return ExtractIdentityIdFromLocationHeader(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    private static string ExtractIdentityIdFromLocationHeader(
        HttpResponseMessage httpResponseMessage)
    {
        const string usersSegmentName = "users/";

        var locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

        if (locationHeader is null)
        {
            throw new InvalidOperationException("Location header can't be null");
        }

        var userSegmentValueIndex = locationHeader.IndexOf(
            usersSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        var userIdentityId = locationHeader.Substring(
            userSegmentValueIndex + usersSegmentName.Length);

        return userIdentityId;
    }
}