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

            var distributorId = await CreateDistributorAsync(distributorPhone);
            var posId = await CreatePointOfSaleAsync(pointOfSalePhone);

            var request = new AssignDistributorToPointOfSaleRequest(
                distributorPhone,
                category.ToString()
            );

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.PostAsJsonAsync($"api/pos/{pointOfSalePhone}/distributors", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the assignment was persisted in the database
            const string sql = @"
                SELECT COUNT(*) 
                FROM point_of_sale_distributor 
                WHERE point_of_sale_id = @PointOfSaleId 
                AND distributor_id = @DistributorId 
                AND category = @Category";

            using var connection = SqlConnectionFactory.CreateConnection();
            var count = await connection.QuerySingleAsync<int>(sql, new
            {
                PointOfSaleId = posId,
                DistributorId = distributorId,
                Category = category.ToString()
            });

            count.Should().Be(1);
        }
    }
}