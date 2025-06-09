using Conaprole.Orders.Application.Abstractions.Authentication;

namespace Conaprole.Orders.Application.IntegrationTests.Infrastructure;

/// <summary>
/// Test implementation of IUserContext that allows setting test values.
/// </summary>
public sealed class TestUserContext : IUserContext
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    
    public string IdentityId { get; set; } = string.Empty;
}