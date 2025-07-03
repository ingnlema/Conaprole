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
        if (string.IsNullOrEmpty(_keycloakOptions.InitialAdminUser) || 
            string.IsNullOrEmpty(_keycloakOptions.InitialAdminPassword))
        {
            _logger.LogInformation("Initial admin user configuration not found or incomplete. Skipping creation.");
            return;
        }

        try
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(_keycloakOptions.InitialAdminUser, cancellationToken);
            if (existingUser is not null)
            {
                _logger.LogInformation("Initial admin user {Email} already exists. Skipping creation.", _keycloakOptions.InitialAdminUser);
                return;
            }

            _logger.LogInformation("Attempting to create initial admin user {Email}.", _keycloakOptions.InitialAdminUser);

            // Try to register the user
            var registerCommand = new RegisterUserCommand(
                _keycloakOptions.InitialAdminUser,
                "Admin",
                "Initial",
                _keycloakOptions.InitialAdminPassword);

            var registerResult = await _sender.Send(registerCommand, cancellationToken);
            
            if (registerResult.IsFailure)
            {
                _logger.LogWarning("Failed to register initial admin user: {Error}", registerResult.Error.Name);
                return;
            }

            var userId = registerResult.Value;
            _logger.LogInformation("Initial admin user registered with ID {UserId}.", userId);

            // Assign Administrator role
            var assignRoleCommand = new AssignRoleCommand(userId, "Administrator");
            var assignRoleResult = await _sender.Send(assignRoleCommand, cancellationToken);

            if (assignRoleResult.IsFailure)
            {
                _logger.LogError("Failed to assign Administrator role to initial admin user: {Error}", assignRoleResult.Error.Name);
                return;
            }

            _logger.LogInformation("Initial admin user {Email} created successfully with Administrator role.", _keycloakOptions.InitialAdminUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create initial admin user {Email}.", _keycloakOptions.InitialAdminUser);
            // Don't throw - this shouldn't prevent the application from starting
        }
    }
}