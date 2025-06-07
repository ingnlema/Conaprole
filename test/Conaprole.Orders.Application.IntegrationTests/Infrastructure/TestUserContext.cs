using Conaprole.Orders.Application.Abstractions.Authentication;

namespace Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

public sealed class TestUserContext : IUserContext
{
    public Guid UserId { get; set; }
    public string IdentityId { get; set; } = string.Empty;
}