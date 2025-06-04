using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Distributors.GetDistributors;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.FunctionalTests.Distributors
{
    [Collection("ApiCollection")]
    public class GetDistributorsTest : BaseFunctionalTest
    {
        public GetDistributorsTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetDistributors_WithExistingDistributors_ShouldReturnAllDistributors()
        {
            // Arrange - Create test distributors
            var distributor1Phone = "+59890000001";
            var distributor2Phone = "+59890000002";
            var pointOfSalePhone1 = "+59891111001";
            var pointOfSalePhone2 = "+59891111002";

            var distributor1Id = await CreateDistributorAsync(distributor1Phone);
            var distributor2Id = await CreateDistributorAsync(distributor2Phone);
            
            var pos1Id = await CreatePointOfSaleAsync(pointOfSalePhone1);
            var pos2Id = await CreatePointOfSaleAsync(pointOfSalePhone2);

            // Assign distributors to points of sale
            await AssignDistributorToPointOfSaleAsync(pos1Id, distributor1Id, Category.LACTEOS);
            await AssignDistributorToPointOfSaleAsync(pos2Id, distributor1Id, Category.CONGELADOS);
            await AssignDistributorToPointOfSaleAsync(pos1Id, distributor2Id, Category.LACTEOS);

            // Act
            var response = await HttpClient.GetAsync("api/distributors");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var distributors = await response.Content.ReadFromJsonAsync<List<DistributorSummaryResponse>>();
            
            distributors.Should().NotBeNull();
            distributors.Should().HaveCountGreaterThan(1);

            // Find our test distributors
            var testDistributor1 = distributors!.FirstOrDefault(d => d.PhoneNumber == distributor1Phone);
            var testDistributor2 = distributors!.FirstOrDefault(d => d.PhoneNumber == distributor2Phone);

            testDistributor1.Should().NotBeNull();
            testDistributor2.Should().NotBeNull();

            // Verify distributor 1 structure and data
            testDistributor1!.Id.Should().Be(distributor1Id);
            testDistributor1.PhoneNumber.Should().Be(distributor1Phone);
            testDistributor1.Name.Should().Be("Distribuidor Test");
            testDistributor1.Address.Should().Be("Calle Falsa 123");
            testDistributor1.CreatedAt.Should().BeAfter(DateTime.MinValue);
            testDistributor1.SupportedCategories.Should().NotBeNull();
            testDistributor1.AssignedPointsOfSaleCount.Should().Be(2); // assigned to 2 different POS

            // Verify distributor 2 structure and data
            testDistributor2!.Id.Should().Be(distributor2Id);
            testDistributor2.PhoneNumber.Should().Be(distributor2Phone);
            testDistributor2.Name.Should().Be("Distribuidor Test");
            testDistributor2.Address.Should().Be("Calle Falsa 123");
            testDistributor2.CreatedAt.Should().BeAfter(DateTime.MinValue);
            testDistributor2.SupportedCategories.Should().NotBeNull();
            testDistributor2.AssignedPointsOfSaleCount.Should().Be(1); // assigned to 1 POS
        }

        [Fact]
        public async Task GetDistributors_WithNoDistributors_ShouldReturnEmptyList()
        {
            // Act
            var response = await HttpClient.GetAsync("api/distributors");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var distributors = await response.Content.ReadFromJsonAsync<List<DistributorSummaryResponse>>();
            
            distributors.Should().NotBeNull();
            distributors.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDistributors_WithDistributorWithoutAssignments_ShouldShowZeroCount()
        {
            // Arrange - Create a distributor without any point of sale assignments
            var distributorPhone = "+59890000003";
            var distributorId = await CreateDistributorAsync(distributorPhone);

            // Act
            var response = await HttpClient.GetAsync("api/distributors");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var distributors = await response.Content.ReadFromJsonAsync<List<DistributorSummaryResponse>>();
            
            distributors.Should().NotBeNull();
            distributors.Should().HaveCount(1);

            var distributor = distributors!.First();
            distributor.Id.Should().Be(distributorId);
            distributor.PhoneNumber.Should().Be(distributorPhone);
            distributor.Name.Should().Be("Distribuidor Test");
            distributor.Address.Should().Be("Calle Falsa 123");
            distributor.AssignedPointsOfSaleCount.Should().Be(0);
            distributor.SupportedCategories.Should().NotBeNull();
        }
    }
}