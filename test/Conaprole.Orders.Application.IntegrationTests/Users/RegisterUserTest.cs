using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class RegisterUserTest : BaseIntegrationTest, IAsyncLifetime
{
    public RegisterUserTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public async Task InitializeAsync()
    {
        // Clean up any existing test users before each test
        var testEmails = new[] { UserData.Email, UserData.AlternativeEmail, "test3@conaprole.com" };
        var existingUsers = await DbContext.Set<User>()
            .Where(u => testEmails.Contains(u.Email.Value))
            .ToListAsync();
        
        if (existingUsers.Any())
        {
            DbContext.Set<User>().RemoveRange(existingUsers);
            await DbContext.SaveChangesAsync();
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RegisterUserCommand_Should_CreateUser_Successfully()
    {
        // Arrange
        var command = UserData.CreateCommand;

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify user was created in database
        var user = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email.Value == UserData.Email);

        Assert.NotNull(user);
        Assert.Equal(UserData.FirstName, user.FirstName.Value);
        Assert.Equal(UserData.LastName, user.LastName.Value);
        Assert.Equal(UserData.Email, user.Email.Value);
        Assert.NotEmpty(user.IdentityId);
        Assert.Null(user.DistributorId);
    }

    [Fact]
    public async Task RegisterUserCommand_WithDistributor_Should_CreateUserWithDistributorAssociation()
    {
        // Arrange - First create a distributor
        var distributorId = await DistributorData.SeedAsync(Sender);
        var command = new RegisterUserCommand(
            UserData.AlternativeEmail,
            UserData.FirstName,
            UserData.LastName,
            UserData.Password,
            DistributorData.PhoneNumber);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify user was created with distributor association
        var user = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email.Value == UserData.AlternativeEmail);

        Assert.NotNull(user);
        Assert.Equal(UserData.FirstName, user.FirstName.Value);
        Assert.Equal(UserData.LastName, user.LastName.Value);
        Assert.Equal(UserData.AlternativeEmail, user.Email.Value);
        Assert.NotEmpty(user.IdentityId);
        Assert.Equal(distributorId, user.DistributorId);
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
        var user = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email.Value == "test3@conaprole.com");

        Assert.Null(user);
    }
}