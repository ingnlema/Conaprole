using Conaprole.Orders.Application.Users.GetLoggedInUser;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;
using Dapper;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class GetLoggedInUserTest : BaseIntegrationTest, IAsyncLifetime
{
    public GetLoggedInUserTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public new async Task InitializeAsync()
    {
        // Call base InitializeAsync to clean database
        await base.InitializeAsync();
        
        // Clean up any existing test users before each test
        var testEmails = new[] { UserData.Email, UserData.AlternativeEmail };
        var existingUsers = await DbContext.Set<User>()
            .Where(u => testEmails.Contains(u.Email.Value))
            .ToListAsync();
        
        if (existingUsers.Any())
        {
            DbContext.Set<User>().RemoveRange(existingUsers);
            await DbContext.SaveChangesAsync();
        }
        
        // Reset TestUserContext
        TestUserContext.IdentityId = string.Empty;
        TestUserContext.UserId = Guid.NewGuid();
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetLoggedInUserQuery_Should_ReturnUser_WhenUserExists()
    {
        // Arrange - Create a user and set up the test context
        var userId = await UserData.SeedAsync(Sender);
        var testIdentityId = "test-identity-123";
        
        // Update the user with the test identity ID
        await SetUserIdentityIdAsync(userId, testIdentityId);
        
        // Configure TestUserContext to return the test identity ID
        TestUserContext.IdentityId = testIdentityId;
        TestUserContext.UserId = userId;

        // Act
        var result = await Sender.Send(new GetLoggedInUserQuery());

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        
        var user = result.Value;
        Assert.Equal(userId, user.Id);
        Assert.Equal(UserData.Email, user.Email);
        Assert.Equal(UserData.FirstName, user.FirstName);
        Assert.Equal(UserData.LastName, user.LastName);
        Assert.Contains("Registered", user.Roles);
        Assert.Null(user.DistributorId);
        Assert.Null(user.DistributorPhoneNumber);
    }

    [Fact]
    public async Task GetLoggedInUserQuery_Should_ReturnUserWithDistributor_WhenUserHasDistributor()
    {
        // Arrange - Create a distributor first
        var distributorId = await DistributorData.SeedAsync(Sender);
        
        // Create a user and associate with distributor
        var userId = await UserData.SeedWithDistributorAsync(Sender, DistributorData.PhoneNumber);
        var testIdentityId = "test-identity-with-distributor-456";
        
        // Update the user with the test identity ID
        await SetUserIdentityIdAsync(userId, testIdentityId);
        
        // Configure TestUserContext to return the test identity ID
        TestUserContext.IdentityId = testIdentityId;
        TestUserContext.UserId = userId;

        // Act
        var result = await Sender.Send(new GetLoggedInUserQuery());

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        
        var user = result.Value;
        Assert.Equal(userId, user.Id);
        Assert.Equal(UserData.Email, user.Email);
        Assert.Equal(UserData.FirstName, user.FirstName);
        Assert.Equal(UserData.LastName, user.LastName);
        Assert.Contains("Registered", user.Roles);
        Assert.Equal(distributorId, user.DistributorId);
        Assert.Equal(DistributorData.PhoneNumber, user.DistributorPhoneNumber);
    }

    [Fact]
    public async Task GetLoggedInUserQuery_Should_ReturnFailure_WhenIdentityIdIsEmpty()
    {
        // Arrange - Don't set an identity ID
        TestUserContext.IdentityId = string.Empty;

        // Act
        var result = await Sender.Send(new GetLoggedInUserQuery());

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetLoggedInUserQuery_Should_ReturnFailure_WhenUserNotFound()
    {
        // Arrange - Set a non-existent identity ID
        TestUserContext.IdentityId = "non-existent-identity";

        // Act
        var result = await Sender.Send(new GetLoggedInUserQuery());

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task SetUserIdentityIdAsync(Guid userId, string identityId)
    {
        const string sql = "UPDATE users SET identity_id = @IdentityId WHERE id = @UserId";
        
        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            IdentityId = identityId
        });
    }
}