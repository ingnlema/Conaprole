using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class RegisterUserRoleTrackingTest : BaseIntegrationTest, IAsyncLifetime
{
    public RegisterUserRoleTrackingTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RegisterUserCommand_Should_NotThrowEntityTrackingException()
    {
        // Arrange
        var email = UserData.GenerateUniqueEmail();
        var command = new RegisterUserCommand(
            email,
            UserData.FirstName,
            UserData.LastName,
            UserData.Password);

        // Act & Assert - This should not throw an InvalidOperationException about entity tracking
        var result = await Sender.Send(command);

        // Verify the command succeeded
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify user was created with the Registered role
        var userEmail = new Domain.Users.Email(email);
        var user = await DbContext.Set<User>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == userEmail);

        Assert.NotNull(user);
        Assert.Contains(user.Roles, r => r.Name == "Registered");
    }

    [Fact]
    public async Task RegisterUserCommand_WithDistributor_Should_NotThrowEntityTrackingException()
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

        // Act & Assert - This should not throw an InvalidOperationException about entity tracking
        var result = await Sender.Send(command);

        // Verify the command succeeded
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify user was created with both Registered and Distributor roles
        var userEmailValue = new Domain.Users.Email(email);
        var user = await DbContext.Set<User>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == userEmailValue);

        Assert.NotNull(user);
        Assert.Contains(user.Roles, r => r.Name == "Registered");
        Assert.Contains(user.Roles, r => r.Name == "Distributor");
        Assert.Equal(2, user.Roles.Count);
    }
}