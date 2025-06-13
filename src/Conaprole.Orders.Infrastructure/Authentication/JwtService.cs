using System.Net.Http.Json;
using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Infrastructure.Authentication.Models;
using Microsoft.Extensions.Options;

namespace Conaprole.Orders.Infrastructure.Authentication;

internal sealed class JwtService : IJwtService
{
    private static readonly Error AuthenticationFailed = new(
        "Keycloak.AuthenticationFailed",
        "Failed to acquire access token do to authentication failure");

    private readonly HttpClient _httpClient;
    private readonly KeycloakOptions _keycloakOptions;

    public JwtService(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions)
    {
        _httpClient = httpClient;
        _keycloakOptions = keycloakOptions.Value;
    }

    public async Task<Result<TokenResult>> GetAccessTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("scope", "openid email"),
                new("grant_type", "password"),
                new("username", email),
                new("password", password)
            };

            var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

            var response = await _httpClient.PostAsync("", authorizationRequestContent, cancellationToken);

            response.EnsureSuccessStatusCode();

            var authorizationToken = await response.Content.ReadFromJsonAsync<AuthorizationToken>();

            if (authorizationToken is null)
            {
                return Result.Failure<TokenResult>(AuthenticationFailed);
            }

            return new TokenResult(authorizationToken.AccessToken, authorizationToken.RefreshToken);
        }
        catch (HttpRequestException)
        {
            return Result.Failure<TokenResult>(AuthenticationFailed);
        }
    }

    public async Task<Result<TokenResult>> GetAccessTokenFromRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.AuthClientId),
                new("client_secret", _keycloakOptions.AuthClientSecret),
                new("grant_type", "refresh_token"),
                new("refresh_token", refreshToken)
            };

            var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

            var response = await _httpClient.PostAsync("", authorizationRequestContent, cancellationToken);

            response.EnsureSuccessStatusCode();

            var authorizationToken = await response.Content.ReadFromJsonAsync<AuthorizationToken>();

            if (authorizationToken is null)
            {
                return Result.Failure<TokenResult>(AuthenticationFailed);
            }

            return new TokenResult(authorizationToken.AccessToken, authorizationToken.RefreshToken);
        }
        catch (HttpRequestException)
        {
            return Result.Failure<TokenResult>(AuthenticationFailed);
        }
    }
}