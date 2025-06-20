using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Application.Exceptions;
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
        catch (HttpRequestException ex)
        {
            // In .NET 5+, HttpRequestException.Data contains the status code
            if (ex.Data.Contains("StatusCode"))
            {
                var statusCode = (HttpStatusCode)ex.Data["StatusCode"]!;
                
                switch (statusCode)
                {
                    case HttpStatusCode.Conflict:
                        throw new ConflictException("User already exists", ex);
                    case HttpStatusCode.BadRequest:
                        throw new ValidationException(new[] { new ValidationError("User", "Invalid user data") });
                    default:
                        throw; // Re-throw for other HTTP status codes
                }
            }
            
            // Fallback: Parse the message for status codes (less reliable but covers edge cases)
            if (ex.Message.Contains("409") || ex.Message.Contains("Conflict"))
            {
                throw new ConflictException("User already exists", ex);
            }
            
            if (ex.Message.Contains("400") || ex.Message.Contains("Bad Request"))
            {
                throw new ValidationException(new[] { new ValidationError("User", "Invalid user data") });
            }

            // For any other HTTP error, re-throw to maintain existing behavior
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    public async Task ChangePasswordAsync(
        string identityId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var credentialRequest = new CredentialRepresentationModel
        {
            Value = newPassword,
            Temporary = false,
            Type = PasswordCredentialType
        };

        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"users/{identityId}/reset-password",
                credentialRequest,
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            // In .NET 5+, HttpRequestException.Data contains the status code
            if (ex.Data.Contains("StatusCode"))
            {
                var statusCode = (HttpStatusCode)ex.Data["StatusCode"]!;
                
                switch (statusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new NotFoundException("User", identityId);
                    case HttpStatusCode.BadRequest:
                        throw new ValidationException(new[] { new ValidationError("Password", "Invalid password data") });
                    default:
                        throw; // Re-throw for other HTTP status codes
                }
            }
            
            // Fallback: Parse the message for status codes (less reliable but covers edge cases)
            if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                throw new NotFoundException("User", identityId);
            }
            
            if (ex.Message.Contains("400") || ex.Message.Contains("Bad Request"))
            {
                throw new ValidationException(new[] { new ValidationError("Password", "Invalid password data") });
            }

            // For any other HTTP error, re-throw to maintain existing behavior
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task DeleteAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"users/{identityId}",
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            // In .NET 5+, HttpRequestException.Data contains the status code
            if (ex.Data.Contains("StatusCode"))
            {
                var statusCode = (HttpStatusCode)ex.Data["StatusCode"]!;
                
                switch (statusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new NotFoundException("User", identityId);
                    default:
                        throw; // Re-throw for other HTTP status codes
                }
            }
            
            // Fallback: Parse the message for status codes (less reliable but covers edge cases)
            if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                throw new NotFoundException("User", identityId);
            }

            // For any other HTTP error, re-throw to maintain existing behavior
            throw;
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