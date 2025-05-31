using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Distributors.GetPointOfSaleDetails;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.FunctionalTests.Distributors
{
    [Collection("ApiCollection")]
    public class GetPointOfSaleDetailsTest : BaseFunctionalTest
    {
        public GetPointOfSaleDetailsTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetPointOfSaleDetails_ShouldReturnOk_WhenPointOfSaleIsAssignedToDistributor()
        {
            // Arrange
            var distributorPhone = "+59899887766";
            var pointOfSalePhone = "+59891234567";

            // Create distributor and point of sale
            var distributorId = await CreateDistributorAsync(distributorPhone);
            var pointOfSaleId = await CreatePointOfSaleAsync(pointOfSalePhone);

            // Assign distributor to point of sale
            await AssignDistributorToPointOfSaleAsync(pointOfSaleId, distributorId, Category.LACTEOS);

            // Act
            var response = await HttpClient.GetAsync(
                $"api/distributors/{Uri.EscapeDataString(distributorPhone)}/pos/{Uri.EscapeDataString(pointOfSalePhone)}"
            );

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pointOfSaleDetails = await response.Content.ReadFromJsonAsync<PointOfSaleDetailsResponse>();
            pointOfSaleDetails.Should().NotBeNull();
            pointOfSaleDetails!.Name.Should().Be("POS de Prueba");
            pointOfSaleDetails.PhoneNumber.Should().Be(pointOfSalePhone);
            pointOfSaleDetails.IsActive.Should().BeTrue();
            pointOfSaleDetails.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task GetPointOfSaleDetails_ShouldReturnNotFound_WhenPointOfSaleIsNotAssignedToDistributor()
        {
            // Arrange
            var distributorPhone = "+59899887766";
            var pointOfSalePhone = "+59891234567";

            // Create distributor and point of sale but don't assign them
            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Act
            var response = await HttpClient.GetAsync(
                $"api/distributors/{Uri.EscapeDataString(distributorPhone)}/pos/{Uri.EscapeDataString(pointOfSalePhone)}"
            );

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}