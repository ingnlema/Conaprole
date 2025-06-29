using Keycloak.Net;
using Keycloak.Net.Models.Clients;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Conaprole.Orders.Domain.Users;
using Conaprole.Orders.Infrastructure.Authentication.Models;
using Conaprole.Orders.Infrastructure.Configuration.SeedData;
using System.Net.Http.Json;
using System.Text.Json;

using KeycloakRole = Keycloak.Net.Models.Roles.Role;
using KeycloakUser = Keycloak.Net.Models.Users.User;

namespace Conaprole.Orders.Infrastructure.Authentication;

internal sealed class KeycloakRealmSeeder : IKeycloakRealmSeeder
{
    private readonly ILogger<KeycloakRealmSeeder> _logger;
    private readonly KeycloakOptions _keycloakOptions;
    private readonly KeycloakClient _keycloakClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    // Test user credentials for GetTokenAsync
    private const string TestUserEmail = "test@conaprole.com";
    private const string TestUserPassword = "TestPassword123";

    public KeycloakRealmSeeder(
        ILogger<KeycloakRealmSeeder> logger,
        IOptions<KeycloakOptions> keycloakOptions)
    {
        _logger = logger;
        _keycloakOptions = keycloakOptions.Value;
        
        _keycloakClient = new KeycloakClient(
            _keycloakOptions.AdminUrl.TrimEnd('/'),
            _keycloakOptions.AdminClientId,
            _keycloakOptions.AdminClientSecret);
    }

    public async Task SeedRealmAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Starting Keycloak realm seeding...");

            await EnsureClientsExistAsync(cancellationToken);
            await EnsureRolesExistAsync(cancellationToken);
            await EnsureRolePermissionMappingsAsync(cancellationToken);
            await EnsureTestUserExistsAsync(cancellationToken);

            _logger.LogInformation("Keycloak realm seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed Keycloak realm");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<string> GetTokenAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure the test user has the required roles
            await EnsureTestUserHasRolesAsync(roles, cancellationToken);

            // Get token using the test user credentials
            var authRequestParameters = new Dictionary<string, string>
            {
                ["client_id"] = _keycloakOptions.AuthClientId,
                ["client_secret"] = _keycloakOptions.AuthClientSecret,
                ["scope"] = "openid email",
                ["grant_type"] = "password",
                ["username"] = TestUserEmail,
                ["password"] = TestUserPassword
            };

            // Use HttpClient to call token endpoint directly
            using var httpClient = new HttpClient();
            var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);
            var response = await httpClient.PostAsync(_keycloakOptions.TokenUrl, authorizationRequestContent, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            var authToken = await response.Content.ReadFromJsonAsync<AuthorizationToken>(cancellationToken);
            return authToken?.AccessToken ?? throw new InvalidOperationException("Failed to obtain access token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get token for roles: {Roles}", string.Join(", ", roles));
            throw;
        }
    }

    private async Task EnsureClientsExistAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ensuring Keycloak clients exist...");

        var existingClients = await _keycloakClient.GetClientsAsync("Conaprole");
        
        // Ensure auth client exists
        if (!existingClients.Any(c => c.ClientId == _keycloakOptions.AuthClientId))
        {
            var authClient = new Client
            {
                ClientId = _keycloakOptions.AuthClientId,
                Name = "Conaprole Auth Client",
                Enabled = true,
                DirectAccessGrantsEnabled = true,
                ServiceAccountsEnabled = false,
                StandardFlowEnabled = true,
                ImplicitFlowEnabled = false,
                ClientAuthenticatorType = "client-secret",
                Secret = _keycloakOptions.AuthClientSecret,
                Protocol = "openid-connect"
            };

            await _keycloakClient.CreateClientAsync("Conaprole", authClient);
            _logger.LogInformation("Created auth client: {ClientId}", _keycloakOptions.AuthClientId);
        }

        // Ensure admin client exists
        if (!existingClients.Any(c => c.ClientId == _keycloakOptions.AdminClientId))
        {
            var adminClient = new Client
            {
                ClientId = _keycloakOptions.AdminClientId,
                Name = "Conaprole Admin Client",
                Enabled = true,
                ServiceAccountsEnabled = true,
                StandardFlowEnabled = false,
                ImplicitFlowEnabled = false,
                DirectAccessGrantsEnabled = false,
                ClientAuthenticatorType = "client-secret",
                Secret = _keycloakOptions.AdminClientSecret,
                Protocol = "openid-connect"
            };

            await _keycloakClient.CreateClientAsync("Conaprole", adminClient);
            _logger.LogInformation("Created admin client: {ClientId}", _keycloakOptions.AdminClientId);
        }
    }

    private async Task EnsureRolesExistAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ensuring Keycloak roles exist...");

        var existingRoles = await _keycloakClient.GetRolesAsync("Conaprole");
        var allRoles = Enum.GetValues<ApplicationRole>();

        foreach (var applicationRole in allRoles)
        {
            var roleName = applicationRole.ToString();
            if (!existingRoles.Any(r => r.Name == roleName))
            {
                var role = new KeycloakRole
                {
                    Name = roleName,
                    Description = $"Application role: {roleName}"
                };

                await _keycloakClient.CreateRoleAsync("Conaprole", role);
                _logger.LogInformation("Created role: {RoleName}", roleName);
            }
        }

        // Also create permission-based roles for fine-grained access
        var allPermissions = Enum.GetValues<ApplicationPermission>();
        foreach (var permission in allPermissions)
        {
            var permissionName = RolePermissionMapping.GetPermissionName(permission);
            if (!existingRoles.Any(r => r.Name == permissionName))
            {
                var role = new KeycloakRole
                {
                    Name = permissionName,
                    Description = $"Permission: {permissionName}"
                };

                await _keycloakClient.CreateRoleAsync("Conaprole", role);
                _logger.LogInformation("Created permission role: {PermissionName}", permissionName);
            }
        }
    }

    private async Task EnsureRolePermissionMappingsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ensuring role-permission mappings...");

        var roles = await _keycloakClient.GetRolesAsync("Conaprole");
        
        // Define role-permission mappings using shared configuration
        var rolePermissionMappings = RolePermissionMapping.GetRolePermissionMappings();

        foreach (var mapping in rolePermissionMappings)
        {
            var roleName = mapping.Key.ToString();
            var role = roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null) continue;

            var permissionRoles = new List<KeycloakRole>();
            foreach (var permission in mapping.Value)
            {
                var permissionName = RolePermissionMapping.GetPermissionName(permission);
                var permissionRole = roles.FirstOrDefault(r => r.Name == permissionName);
                if (permissionRole != null)
                {
                    permissionRoles.Add(permissionRole);
                }
            }

            if (permissionRoles.Any())
            {
                try
                {
                    await _keycloakClient.AddCompositesToRoleAsync("Conaprole", roleName, permissionRoles);
                    _logger.LogInformation("Added composite roles to {RoleName}: {Permissions}", 
                        roleName, string.Join(", ", permissionRoles.Select(p => p.Name)));
                }
                catch (Exception ex)
                {
                    // This might fail if composites already exist, which is fine for idempotent operation
                    _logger.LogDebug(ex, "Failed to add composites to role {RoleName}, they may already exist", roleName);
                }
            }
        }
    }

    private async Task EnsureTestUserExistsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ensuring test user exists...");

        try
        {
            var users = await _keycloakClient.GetUsersAsync("Conaprole", email: TestUserEmail);
            if (!users.Any())
            {
                var testUser = new KeycloakUser
                {
                    UserName = TestUserEmail,
                    Email = TestUserEmail,
                    FirstName = "Test",
                    LastName = "User",
                    Enabled = true,
                    EmailVerified = true,
                    Credentials = new List<Credentials>
                    {
                        new()
                        {
                            Type = "password",
                            Value = TestUserPassword,
                            Temporary = false
                        }
                    }
                };

                await _keycloakClient.CreateUserAsync("Conaprole", testUser);
                _logger.LogInformation("Created test user: {Email}", TestUserEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ensure test user exists, this might be expected in some environments");
        }
    }

    private async Task EnsureTestUserHasRolesAsync(IEnumerable<string> requiredRoles, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _keycloakClient.GetUsersAsync("Conaprole", email: TestUserEmail);
            var testUser = users.FirstOrDefault();
            if (testUser == null)
            {
                await EnsureTestUserExistsAsync(cancellationToken);
                users = await _keycloakClient.GetUsersAsync("Conaprole", email: TestUserEmail);
                testUser = users.FirstOrDefault();
            }

            if (testUser == null) return;

            var allRoles = await _keycloakClient.GetRolesAsync("Conaprole");
            var rolesToAssign = new List<KeycloakRole>();

            foreach (var roleName in requiredRoles)
            {
                var role = allRoles.FirstOrDefault(r => r.Name == roleName);
                if (role != null)
                {
                    rolesToAssign.Add(role);
                }
            }

            if (rolesToAssign.Any())
            {
                try
                {
                    await _keycloakClient.AddRealmRoleMappingsToUserAsync("Conaprole", testUser.Id, rolesToAssign);
                    _logger.LogDebug("Assigned roles to test user: {Roles}", string.Join(", ", rolesToAssign.Select(r => r.Name)));
                }
                catch (Exception ex)
                {
                    // Roles might already be assigned, which is fine
                    _logger.LogDebug(ex, "Failed to assign roles to test user, they might already be assigned");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ensure test user has required roles");
        }
    }
}