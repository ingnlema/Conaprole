using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;
using Dapper;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale
{
    [Collection("ApiCollection")]
    public class AssignDistributorTest : BaseFunctionalTest
    {
        public AssignDistributorTest(FunctionalTestWebAppFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task AssignDistributor_ShouldReturnNoContent_WhenAssignmentIsSuccessful()
        {
            // Arrange
            var distributorPhone = "+59899887766";
            var pointOfSalePhone = "+59891234567";
            var category = Category.LACTEOS;

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            var request = new AssignDistributorToPointOfSaleRequest(
                distributorPhone,
                category.ToString()
            );

            // Act
            var response = await HttpClient.PostAsJsonAsync($"api/pos/{pointOfSalePhone}/distributors", request);

            // Assert
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Expected NoContent but got {response.StatusCode}. Content: {errorContent}");
            }
            
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}