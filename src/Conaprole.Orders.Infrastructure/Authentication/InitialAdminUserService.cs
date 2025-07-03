using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Conaprole.Orders.Infrastructure.Authentication;

public interface IInitialAdminUserService
{
    Task CreateInitialAdminUserAsync(CancellationToken cancellationToken = default);
}

internal sealed class InitialAdminUserService : IInitialAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly KeycloakOptions _keycloakOptions;
    private readonly ILogger<InitialAdminUserService> _logger;

    public InitialAdminUserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuthenticationService authenticationService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IOptions<KeycloakOptions> keycloakOptions,
        ILogger<InitialAdminUserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _authenticationService = authenticationService;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
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

            // Get Administrator role
            var adminRole = await _roleRepository.GetByNameAsync("Administrator", cancellationToken);
            if (adminRole is null)
            {
                _logger.LogError("Administrator role not found. Cannot create initial admin user.");
                return;
            }

            // Create user
            var user = User.Create(
                new FirstName("Admin"),
                new LastName("Initial"),
                new Email(_keycloakOptions.InitialAdminUser),
                _dateTimeProvider.UtcNow);

            // Assign Administrator role
            user.AssignRole(adminRole);

            // Register user in Keycloak
            var identityId = await _authenticationService.RegisterAsync(
                user,
                _keycloakOptions.InitialAdminPassword,
                cancellationToken);

            user.SetIdentityId(identityId);

            // Save to database
            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Initial admin user {Email} created successfully.", _keycloakOptions.InitialAdminUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create initial admin user {Email}.", _keycloakOptions.InitialAdminUser);
            // Don't throw - this shouldn't prevent the application from starting
        }
    }
}