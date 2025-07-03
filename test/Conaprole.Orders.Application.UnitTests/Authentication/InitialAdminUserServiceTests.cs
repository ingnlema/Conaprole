using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;
using Conaprole.Orders.Infrastructure.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Conaprole.Orders.Application.UnitTests.Authentication;

public class InitialAdminUserServiceTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IRoleRepository _roleRepositoryMock;
    private readonly IAuthenticationService _authenticationServiceMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;
    private readonly KeycloakOptions _keycloakOptions;
    private readonly InitialAdminUserService _service;

    public InitialAdminUserServiceTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _roleRepositoryMock = Substitute.For<IRoleRepository>();
        _authenticationServiceMock = Substitute.For<IAuthenticationService>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();

        _keycloakOptions = new KeycloakOptions
        {
            InitialAdminUser = "admin@test.com",
            InitialAdminPassword = "testpassword"
        };

        var optionsMock = Substitute.For<IOptions<KeycloakOptions>>();
        optionsMock.Value.Returns(_keycloakOptions);

        _service = new InitialAdminUserService(
            _userRepositoryMock,
            _roleRepositoryMock,
            _authenticationServiceMock,
            _unitOfWorkMock,
            _dateTimeProviderMock,
            optionsMock,
            NullLogger<InitialAdminUserService>.Instance);

        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateInitialAdminUserAsync_ShouldSkip_WhenConfigurationIsEmpty()
    {
        // Arrange
        var emptyOptions = new KeycloakOptions { InitialAdminUser = "", InitialAdminPassword = "" };
        var optionsMock = Substitute.For<IOptions<KeycloakOptions>>();
        optionsMock.Value.Returns(emptyOptions);

        var service = new InitialAdminUserService(
            _userRepositoryMock,
            _roleRepositoryMock,
            _authenticationServiceMock,
            _unitOfWorkMock,
            _dateTimeProviderMock,
            optionsMock,
            NullLogger<InitialAdminUserService>.Instance);

        // Act
        await service.CreateInitialAdminUserAsync();

        // Assert
        await _userRepositoryMock.DidNotReceive().GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInitialAdminUserAsync_ShouldSkip_WhenUserAlreadyExists()
    {
        // Arrange
        var existingUser = User.Create(
            new FirstName("Existing"),
            new LastName("User"),
            new Email(_keycloakOptions.InitialAdminUser),
            DateTime.UtcNow);

        _userRepositoryMock.GetByEmailAsync(_keycloakOptions.InitialAdminUser, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        await _service.CreateInitialAdminUserAsync();

        // Assert
        await _authenticationServiceMock.DidNotReceive().RegisterAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _userRepositoryMock.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task CreateInitialAdminUserAsync_ShouldCreateUser_WhenValidConfiguration()
    {
        // Arrange
        _userRepositoryMock.GetByEmailAsync(_keycloakOptions.InitialAdminUser, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        _roleRepositoryMock.GetByNameAsync("Administrator", Arg.Any<CancellationToken>())
            .Returns(Role.Administrator);

        _authenticationServiceMock.RegisterAsync(Arg.Any<User>(), _keycloakOptions.InitialAdminPassword, Arg.Any<CancellationToken>())
            .Returns("test-identity-id");

        // Act
        await _service.CreateInitialAdminUserAsync();

        // Assert
        await _authenticationServiceMock.Received(1).RegisterAsync(Arg.Any<User>(), _keycloakOptions.InitialAdminPassword, Arg.Any<CancellationToken>());
        _userRepositoryMock.Received(1).Add(Arg.Any<User>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateInitialAdminUserAsync_ShouldNotThrow_WhenExceptionOccurs()
    {
        // Arrange
        _userRepositoryMock.GetByEmailAsync(_keycloakOptions.InitialAdminUser, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<User?>(new Exception("Database error")));

        // Act & Assert - Should not throw
        await _service.CreateInitialAdminUserAsync();

        // The test passes if no exception is thrown
    }
}