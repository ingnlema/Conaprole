using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class RegisterUserTest : BaseIntegrationTest, IAsyncLifetime
{
    public RegisterUserTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public new async Task InitializeAsync()
    {
        // Call base InitializeAsync to clean database
        await base.InitializeAsync();
        
        // No need to clean up specific users since we're using unique emails
        // Database cleanup is handled by base.InitializeAsync()
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RegisterUserCommand_Should_CreateUser_Successfully()
    {
        // Arrange
        var email = UserData.GenerateUniqueEmail();
        var command = new RegisterUserCommand(
            email,
            UserData.FirstName,
            UserData.LastName,
            UserData.Password);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify user was created in database
        var userEmail = new Domain.Users.Email(email);
        var user = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == userEmail);

        Assert.NotNull(user);
        Assert.Equal(UserData.FirstName, user.FirstName.Value);
        Assert.Equal(UserData.LastName, user.LastName.Value);
        Assert.Equal(email, user.Email.Value);
        Assert.NotEmpty(user.IdentityId);
        Assert.Null(user.DistributorId);
        
        // Verify user has only the Registered role
        Assert.Contains(Domain.Users.Role.Registered, user.Roles);
        Assert.DoesNotContain(Domain.Users.Role.Distributor, user.Roles);
        Assert.Single(user.Roles);
    }

    [Fact]
    public async Task RegisterUserCommand_WithDistributor_Should_CreateUserWithDistributorAssociation()
    {
        // Arrange - First create a distributor
        var distributorId = await DistributorData.SeedAsync(Sender);
        var email = UserData.GenerateUniqueAlternativeEmail();
        var command = new RegisterUserCommand(
            email,
            UserData.FirstName,
            UserData.LastName,
            UserData.Password,
            DistributorData.PhoneNumber);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify user was created with distributor association and Distributor role
        var alternativeUserEmail = new Domain.Users.Email(email);
        var user = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == alternativeUserEmail);

        Assert.NotNull(user);
        Assert.Equal(UserData.FirstName, user.FirstName.Value);
        Assert.Equal(UserData.LastName, user.LastName.Value);
        Assert.Equal(email, user.Email.Value);
        Assert.NotEmpty(user.IdentityId);
        Assert.Equal(distributorId, user.DistributorId);
        
        // Verify user has both Registered and Distributor roles
        Assert.Contains(Domain.Users.Role.Registered, user.Roles);
        Assert.Contains(Domain.Users.Role.Distributor, user.Roles);
        Assert.Equal(2, user.Roles.Count);
    }

    [Fact]
    public async Task RegisterUserCommand_WithInvalidDistributor_Should_Fail()
    {
        // Arrange
        var invalidPhoneNumber = "+59811111111";
        var command = new RegisterUserCommand(
            "test3@conaprole.com",
            UserData.FirstName,
            UserData.LastName,
            UserData.Password,
            invalidPhoneNumber);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
        
        // Verify no user was created
        var testUserEmail = new Domain.Users.Email("test3@conaprole.com");
        var user = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == testUserEmail);

        Assert.Null(user);
    }
}