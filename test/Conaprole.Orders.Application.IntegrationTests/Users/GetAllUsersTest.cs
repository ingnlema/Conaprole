using Conaprole.Orders.Application.Users.GetAllUsers;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class GetAllUsersTest : BaseIntegrationTest, IAsyncLifetime
{
    public GetAllUsersTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public new async Task InitializeAsync()
    {
        // Call base InitializeAsync to clean database
        await base.InitializeAsync();
        
        // Clean up any existing test users before each test
        var testEmails = new[] { UserData.Email, UserData.AlternativeEmail, "admin@conaprole.com" };
        var existingUsers = await DbContext.Set<User>()
            .Where(u => testEmails.Contains(u.Email.Value))
            .ToListAsync();
        
        if (existingUsers.Any())
        {
            DbContext.Set<User>().RemoveRange(existingUsers);
            await DbContext.SaveChangesAsync();
        }
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetAllUsersQuery_Should_ReturnAllUsers_WhenNoFilter()
    {
        // Arrange - Create test users
        var user1Id = await UserData.SeedAsync(Sender);
        var user2Id = await CreateAlternativeUserAsync();

        // Act
        var result = await Sender.Send(new GetAllUsersQuery());

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        
        var users = result.Value;
        Assert.True(users.Count >= 2); // At least our 2 test users
        
        var testUser1 = users.FirstOrDefault(u => u.Id == user1Id);
        var testUser2 = users.FirstOrDefault(u => u.Id == user2Id);
        
        Assert.NotNull(testUser1);
        Assert.NotNull(testUser2);
        
        Assert.Equal(UserData.Email, testUser1.Email);
        Assert.Equal(UserData.FirstName, testUser1.FirstName);
        Assert.Equal(UserData.LastName, testUser1.LastName);
        Assert.Contains("Registered", testUser1.Roles);
        
        Assert.Equal(UserData.AlternativeEmail, testUser2.Email);
        Assert.Contains("Registered", testUser2.Roles);
    }

    [Fact]
    public async Task GetAllUsersQuery_Should_ReturnFilteredUsers_WhenRoleFilterProvided()
    {
        // Arrange - Create test users
        var user1Id = await UserData.SeedAsync(Sender);
        var user2Id = await CreateAlternativeUserAsync();

        // Act - Filter by Registered role
        var result = await Sender.Send(new GetAllUsersQuery("Registered"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        
        var users = result.Value;
        Assert.True(users.Count >= 2); // At least our 2 test users
        
        // All returned users should have the Registered role
        Assert.All(users, user => Assert.Contains("Registered", user.Roles));
    }

    [Fact]
    public async Task GetAllUsersQuery_Should_ReturnEmptyList_WhenFilterRoleNotExists()
    {
        // Arrange - Create test users
        await UserData.SeedAsync(Sender);

        // Act - Filter by non-existent role
        var result = await Sender.Send(new GetAllUsersQuery("NonExistentRole"));

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    private async Task<Guid> CreateAlternativeUserAsync()
    {
        var result = await Sender.Send(UserData.CreateAlternativeCommand);
        if (result.IsFailure)
            throw new Exception($"Error creating alternative user: {result.Error.Code}");
        return result.Value;
    }
}