using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Orders.Application.Users.AssignRole;
using Conaprole.Orders.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Conaprole.Orders.Infrastructure.Authentication;

public interface IInitialAdminUserService
{
    Task CreateInitialAdminUserAsync(CancellationToken cancellationToken = default);
}

internal sealed class InitialAdminUserService : IInitialAdminUserService
{
    private readonly ISender _sender;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly KeycloakOptions _keycloakOptions;
    private readonly ILogger<InitialAdminUserService> _logger;

    public InitialAdminUserService(
        ISender sender,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IOptions<KeycloakOptions> keycloakOptions,
        ILogger<InitialAdminUserService> logger)
    {
        _sender = sender;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _keycloakOptions = keycloakOptions.Value;
        _logger = logger;
    }

    public async Task CreateInitialAdminUserAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("==== STARTING InitialAdminUserService.CreateInitialAdminUserAsync ====");
        
        if (string.IsNullOrEmpty(_keycloakOptions.InitialAdminUser) || 
            string.IsNullOrEmpty(_keycloakOptions.InitialAdminPassword))
        {
            _logger.LogWarning("Initial admin user configuration not found or incomplete. InitialAdminUser: '{User}', InitialAdminPassword: '{HasPassword}'", 
                _keycloakOptions.InitialAdminUser, 
                string.IsNullOrEmpty(_keycloakOptions.InitialAdminPassword) ? "Empty" : "Set");
            return;
        }

        _logger.LogInformation("Configuration validated. InitialAdminUser: '{Email}'", _keycloakOptions.InitialAdminUser);

        // Check if Administrator role exists
        _logger.LogInformation("Checking if Administrator role exists in database...");
        var adminRole = await _roleRepository.GetByNameAsync("Administrator", cancellationToken);
        if (adminRole == null)
        {
            _logger.LogError("CRITICAL: Administrator role not found in database. Cannot create admin user.");
            return;
        }
        _logger.LogInformation("Administrator role found with ID: {RoleId}", adminRole.Id);

        // Add a delay to ensure Keycloak is fully ready
        _logger.LogInformation("Waiting 10 seconds to ensure Keycloak is fully ready...");
        await Task.Delay(10000, cancellationToken);

        const int maxRetries = 3;
        const int retryDelayMs = 5000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            _logger.LogInformation("==== ATTEMPT {Attempt} of {MaxRetries} ====", attempt, maxRetries);
            
            try
            {
                var success = await AttemptCreateUserAsync(cancellationToken);
                if (success)
                {
                    _logger.LogInformation("✅ SUCCESS: Initial admin user creation completed successfully on attempt {Attempt}", attempt);
                    return;
                }
                
                if (attempt < maxRetries)
                {
                    _logger.LogWarning("❌ Attempt {Attempt} failed. Retrying in {DelayMs}ms...", attempt, retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 EXCEPTION on attempt {Attempt}: {ExceptionType} - {Message}", 
                    attempt, ex.GetType().Name, ex.Message);
                
                if (attempt < maxRetries)
                {
                    _logger.LogInformation("🔄 Retrying in {DelayMs}ms...", retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
        }
        
        _logger.LogError("💀 FAILED: All {MaxRetries} attempts to create initial admin user have failed", maxRetries);
    }

    private async Task<bool> AttemptCreateUserAsync(CancellationToken cancellationToken)
    {
        var email = _keycloakOptions.InitialAdminUser;
        
        // Check if user already exists
        _logger.LogInformation("🔍 Checking if user {Email} already exists in database...", email);
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
        {
            _logger.LogInformation("✅ Initial admin user {Email} already exists in database (ID: {UserId}). Skipping creation.", 
                email, existingUser.Id);
            return true; // User already exists, consider this success
        }

        _logger.LogInformation("👤 User does not exist. Attempting to create initial admin user {Email}.", email);

        // Try to register the user
        var registerCommand = new RegisterUserCommand(
            email,
            "Admin",
            "Initial",
            _keycloakOptions.InitialAdminPassword);

        _logger.LogInformation("📤 Sending RegisterUserCommand for {Email}...", email);
        var registerResult = await _sender.Send(registerCommand, cancellationToken);
        
        if (registerResult.IsFailure)
        {
            _logger.LogError("❌ Failed to register initial admin user: Error Code: '{ErrorCode}', Error Name: '{ErrorName}'",
                registerResult.Error.Code, registerResult.Error.Name);
            return false;
        }

        var userId = registerResult.Value;
        _logger.LogInformation("✅ Initial admin user registered successfully with ID {UserId}. Now assigning Administrator role...", userId);

        // Verify user was actually created in database
        _logger.LogInformation("🔍 Verifying user was created in database...");
        var createdUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (createdUser == null)
        {
            _logger.LogError("❌ User registration succeeded but user not found in database with ID {UserId}", userId);
            return false;
        }
        _logger.LogInformation("✅ User confirmed in database: {Email} (ID: {UserId})", createdUser.Email.Value, createdUser.Id);

        // Assign Administrator role
        var assignRoleCommand = new AssignRoleCommand(userId, "Administrator");
        _logger.LogInformation("📤 Sending AssignRoleCommand for user {UserId} with role 'Administrator'...", userId);
        var assignRoleResult = await _sender.Send(assignRoleCommand, cancellationToken);

        if (assignRoleResult.IsFailure)
        {
            _logger.LogError("❌ Failed to assign Administrator role to initial admin user: Error Code: '{ErrorCode}', Error Name: '{ErrorName}'",
                assignRoleResult.Error.Code, assignRoleResult.Error.Name);
            return false;
        }

        _logger.LogInformation("✅ Administrator role assigned successfully to user {UserId}", userId);

        // Final verification - reload user with roles
        _logger.LogInformation("🔍 Final verification - reloading user with roles...");
        var finalUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (finalUser == null)
        {
            _logger.LogError("❌ Final verification failed: User not found");
            return false;
        }

        _logger.LogInformation("✅ Final user verification: {Email} (ID: {UserId}) with {RoleCount} roles", 
            finalUser.Email.Value, finalUser.Id, finalUser.Roles.Count);
        
        foreach (var role in finalUser.Roles)
        {
            _logger.LogInformation("  📋 Role: {RoleName} (ID: {RoleId})", role.Name, role.Id);
        }

        var hasAdminRole = finalUser.Roles.Any(r => r.Name == "Administrator");
        if (!hasAdminRole)
        {
            _logger.LogError("❌ User created but Administrator role not found in user's roles");
            return false;
        }

        _logger.LogInformation("🎉 SUCCESS: Initial admin user {Email} created successfully with Administrator role. User ID: {UserId}", 
            email, userId);
        return true;
    }
}