using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Application.Users.ChangePassword;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;
using NSubstitute;

namespace Conaprole.Orders.Application.UnitTests.Users;

public class ChangePasswordCommandHandlerTest
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CurrentUserId = Guid.NewGuid(); 
    private static readonly string IdentityId = "identity-123";
    private static readonly string CurrentUserIdentityId = "current-identity-123";
    private static readonly string NewPassword = "newpassword123";
    
    private static readonly ChangePasswordCommand Command = new(UserId, NewPassword);
    
    private readonly ChangePasswordCommandHandler _handler;
    private readonly IAuthenticationService _authenticationServiceMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IAuthorizationService _authorizationServiceMock;

    public ChangePasswordCommandHandlerTest()
    {
        _authenticationServiceMock = Substitute.For<IAuthenticationService>();
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _userContextMock = Substitute.For<IUserContext>();
        _authorizationServiceMock = Substitute.For<IAuthorizationService>();

        _handler = new ChangePasswordCommandHandler(
            _authenticationServiceMock,
            _userRepositoryMock,
            _userContextMock,
            _authorizationServiceMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserChangesOwnPassword()
    {
        // Arrange
        var user = CreateUser();
        _userRepositoryMock.GetByIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        
        _userContextMock.UserId.Returns(UserId); // Same user changing their own password
        _userContextMock.IdentityId.Returns(CurrentUserIdentityId);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _authenticationServiceMock.Received(1)
            .ChangePasswordAsync(IdentityId, NewPassword, Arg.Any<CancellationToken>());
        
        // Should not check permissions for own password
        await _authorizationServiceMock.DidNotReceive()
            .GetPermissionsForUserAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserWithWritePermissionChangesAnotherPassword()
    {
        // Arrange
        var user = CreateUser();
        _userRepositoryMock.GetByIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        
        _userContextMock.UserId.Returns(CurrentUserId); // Different user
        _userContextMock.IdentityId.Returns(CurrentUserIdentityId);
        
        var permissions = new HashSet<string> { "users:write" };
        _authorizationServiceMock.GetPermissionsForUserAsync(CurrentUserIdentityId)
            .Returns(permissions);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _authenticationServiceMock.Received(1)
            .ChangePasswordAsync(IdentityId, NewPassword, Arg.Any<CancellationToken>());
        
        await _authorizationServiceMock.Received(1)
            .GetPermissionsForUserAsync(CurrentUserIdentityId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserWithoutWritePermissionTriesToChangeAnotherPassword()
    {
        // Arrange
        var user = CreateUser();
        _userRepositoryMock.GetByIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        
        _userContextMock.UserId.Returns(CurrentUserId); // Different user
        _userContextMock.IdentityId.Returns(CurrentUserIdentityId);
        
        var permissions = new HashSet<string> { "users:read" }; // No write permission
        _authorizationServiceMock.GetPermissionsForUserAsync(CurrentUserIdentityId)
            .Returns(permissions);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Authorization.Insufficient", result.Error.Code);
        
        await _authenticationServiceMock.DidNotReceive()
            .ChangePasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        
        await _authorizationServiceMock.Received(1)
            .GetPermissionsForUserAsync(CurrentUserIdentityId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock.GetByIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Found", result.Error.Code); // The typo in UserErrors.NotFound
        
        await _authenticationServiceMock.DidNotReceive()
            .ChangePasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static User CreateUser()
    {
        var user = User.Create(
            new FirstName("Test"),
            new LastName("User"),
            new Email("test@example.com"));
        
        user.SetIdentityId(IdentityId);
        
        return user;
    }
}