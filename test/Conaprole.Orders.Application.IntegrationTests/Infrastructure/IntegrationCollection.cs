
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Infrastructure
{
    [CollectionDefinition("IntegrationCollection")]
    public class IntegrationCollection : ICollectionFixture<IntegrationTestWebAppFactory>
    {
        // no impl needed
    }
}
