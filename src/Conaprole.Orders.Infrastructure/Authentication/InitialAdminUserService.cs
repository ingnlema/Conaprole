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
    private readonly KeycloakOptions _keycloakOptions;
    private readonly ILogger<InitialAdminUserService> _logger;

    public InitialAdminUserService(
        ISender sender,
        IUserRepository userRepository,
        IOptions<KeycloakOptions> keycloakOptions,
        ILogger<InitialAdminUserService> logger)
    {
        _sender = sender;
        _userRepository = userRepository;
        _keycloakOptions = keycloakOptions.Value;
        _logger = logger;
    }

    public async Task CreateInitialAdminUserAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting InitialAdminUserService.CreateInitialAdminUserAsync");
        
        if (string.IsNullOrEmpty(_keycloakOptions.InitialAdminUser) || 
            string.IsNullOrEmpty(_keycloakOptions.InitialAdminPassword))
        {
            _logger.LogWarning("Initial admin user configuration not found or incomplete. InitialAdminUser: '{User}', InitialAdminPassword: '{HasPassword}'", 
                _keycloakOptions.InitialAdminUser, 
                string.IsNullOrEmpty(_keycloakOptions.InitialAdminPassword) ? "Empty" : "Set");
            return;
        }

        // Add a delay to ensure Keycloak is fully ready
        _logger.LogInformation("Waiting 10 seconds to ensure Keycloak is fully ready...");
        await Task.Delay(10000, cancellationToken);

        const int maxRetries = 3;
        const int retryDelayMs = 5000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            _logger.LogInformation("Attempt {Attempt} of {MaxRetries} to create initial admin user", attempt, maxRetries);
            
            try
            {
                var success = await AttemptCreateUserAsync(cancellationToken);
                if (success)
                {
                    _logger.LogInformation("SUCCESS: Initial admin user creation completed successfully on attempt {Attempt}", attempt);
                    return;
                }
                
                if (attempt < maxRetries)
                {
                    _logger.LogWarning("Attempt {Attempt} failed. Retrying in {DelayMs}ms...", attempt, retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EXCEPTION on attempt {Attempt}: Failed to create initial admin user. Exception Type: {ExceptionType}, Message: {Message}", 
                    attempt, ex.GetType().Name, ex.Message);
                
                if (attempt < maxRetries)
                {
                    _logger.LogInformation("Retrying in {DelayMs}ms...", retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
        }
        
        _logger.LogError("FAILED: All {MaxRetries} attempts to create initial admin user have failed", maxRetries);
    }

    private async Task<bool> AttemptCreateUserAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuration found: InitialAdminUser = '{Email}'", _keycloakOptions.InitialAdminUser);
        
        // Check if user already exists
        _logger.LogInformation("Checking if user {Email} already exists in database...", _keycloakOptions.InitialAdminUser);
        var existingUser = await _userRepository.GetByEmailAsync(_keycloakOptions.InitialAdminUser, cancellationToken);
        if (existingUser is not null)
        {
            _logger.LogInformation("Initial admin user {Email} already exists in database (ID: {UserId}). Skipping creation.", 
                _keycloakOptions.InitialAdminUser, existingUser.Id);
            return true; // User already exists, consider this success
        }

        _logger.LogInformation("User does not exist. Attempting to create initial admin user {Email}.", _keycloakOptions.InitialAdminUser);

        // Try to register the user
        var registerCommand = new RegisterUserCommand(
            _keycloakOptions.InitialAdminUser,
            "Admin",
            "Initial",
            _keycloakOptions.InitialAdminPassword);

        _logger.LogInformation("Sending RegisterUserCommand for {Email}...", _keycloakOptions.InitialAdminUser);
        var registerResult = await _sender.Send(registerCommand, cancellationToken);
        
        if (registerResult.IsFailure)
        {
            _logger.LogError("Failed to register initial admin user: Error Code: {ErrorCode}, Error Name: {ErrorName}",
                registerResult.Error.Code, registerResult.Error.Name);
            return false;
        }

        var userId = registerResult.Value;
        _logger.LogInformation("Initial admin user registered successfully with ID {UserId}. Now assigning Administrator role...", userId);

        // Assign Administrator role
        var assignRoleCommand = new AssignRoleCommand(userId, "Administrator");
        _logger.LogInformation("Sending AssignRoleCommand for user {UserId} with role 'Administrator'...", userId);
        var assignRoleResult = await _sender.Send(assignRoleCommand, cancellationToken);

        if (assignRoleResult.IsFailure)
        {
            _logger.LogError("Failed to assign Administrator role to initial admin user: Error Code: {ErrorCode}, Error Name: {ErrorName}",
                assignRoleResult.Error.Code, assignRoleResult.Error.Name);
            return false;
        }

        _logger.LogInformation("SUCCESS: Initial admin user {Email} created successfully with Administrator role. User ID: {UserId}", 
            _keycloakOptions.InitialAdminUser, userId);
        return true;
    }
}