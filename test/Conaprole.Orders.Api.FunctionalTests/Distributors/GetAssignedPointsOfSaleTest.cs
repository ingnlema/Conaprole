using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Distributors.GetAssignedPointsOfSale;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.FunctionalTests.Distributors
{
    [Collection("ApiCollection")]
    public class GetAssignedPointsOfSaleTest : BaseFunctionalTest
    {
        public GetAssignedPointsOfSaleTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetAssignedPointsOfSale_WithValidDistributor_ShouldReturnAssignedPOS()
        {
            // Arrange
            var distributorPhone = "+59890000001";
            var pointOfSalePhone1 = "+59891111001";
            var pointOfSalePhone2 = "+59891111002";
            var unassignedPOSPhone = "+59891111003";

            var distributorId = await CreateDistributorAsync(distributorPhone);
            var posId1 = await CreatePointOfSaleAsync(pointOfSalePhone1);
            var posId2 = await CreatePointOfSaleAsync(pointOfSalePhone2);
            var unassignedPOSId = await CreatePointOfSaleAsync(unassignedPOSPhone);

            // Assign distributor to POS 1 and 2, but not to the third one
            await AssignDistributorToPointOfSaleAsync(posId1, distributorId, Category.LACTEOS);
            await AssignDistributorToPointOfSaleAsync(posId2, distributorId, Category.CONGELADOS);

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.GetAsync($"api/distributors/{distributorPhone}/pos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pointsOfSale = await response.Content.ReadFromJsonAsync<List<PointOfSaleResponse>>();
            
            pointsOfSale.Should().NotBeNull();
            pointsOfSale.Should().HaveCount(2);
            pointsOfSale.Should().Contain(pos => pos.PhoneNumber == pointOfSalePhone1);
            pointsOfSale.Should().Contain(pos => pos.PhoneNumber == pointOfSalePhone2);
            pointsOfSale.Should().NotContain(pos => pos.PhoneNumber == unassignedPOSPhone);

            // Verify the structure of returned objects
            foreach (var pos in pointsOfSale)
            {
                pos.Id.Should().NotBeEmpty();
                pos.Name.Should().NotBeNullOrEmpty();
                pos.PhoneNumber.Should().NotBeNullOrEmpty();
                pos.Address.Should().NotBeNull();
                pos.Address.City.Should().NotBeNullOrEmpty();
                pos.Address.Street.Should().NotBeNullOrEmpty();
                pos.Address.ZipCode.Should().NotBeNullOrEmpty();
                pos.CreatedAt.Should().BeAfter(DateTime.MinValue);
            }
        }

        [Fact]
        public async Task GetAssignedPointsOfSale_WithNonExistentDistributor_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistentDistributorPhone = "+59899999999";

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.GetAsync($"api/distributors/{nonExistentDistributorPhone}/pos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pointsOfSale = await response.Content.ReadFromJsonAsync<List<PointOfSaleResponse>>();
            
            pointsOfSale.Should().NotBeNull();
            pointsOfSale.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAssignedPointsOfSale_WithDistributorWithoutAssignments_ShouldReturnEmptyList()
        {
            // Arrange
            var distributorPhone = "+59890000002";
            await CreateDistributorAsync(distributorPhone);

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.GetAsync($"api/distributors/{distributorPhone}/pos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pointsOfSale = await response.Content.ReadFromJsonAsync<List<PointOfSaleResponse>>();
            
            pointsOfSale.Should().NotBeNull();
            pointsOfSale.Should().BeEmpty();
        }
    }
}