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

        try
        {
            _logger.LogInformation("Configuration found: InitialAdminUser = '{Email}'", _keycloakOptions.InitialAdminUser);
            
            // Check if user already exists
            _logger.LogInformation("Checking if user {Email} already exists in database...", _keycloakOptions.InitialAdminUser);
            var existingUser = await _userRepository.GetByEmailAsync(_keycloakOptions.InitialAdminUser, cancellationToken);
            if (existingUser is not null)
            {
                _logger.LogInformation("Initial admin user {Email} already exists in database (ID: {UserId}). Skipping creation.", 
                    _keycloakOptions.InitialAdminUser, existingUser.Id);
                return;
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
                return;
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
                return;
            }

            _logger.LogInformation("SUCCESS: Initial admin user {Email} created successfully with Administrator role. User ID: {UserId}", 
                _keycloakOptions.InitialAdminUser, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EXCEPTION: Failed to create initial admin user {Email}. Exception Type: {ExceptionType}, Message: {Message}", 
                _keycloakOptions.InitialAdminUser, ex.GetType().Name, ex.Message);
            // Don't throw - this shouldn't prevent the application from starting
        }
    }
}