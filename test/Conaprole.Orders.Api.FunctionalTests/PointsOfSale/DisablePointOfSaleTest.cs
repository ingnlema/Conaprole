using System.Net;
using FluentAssertions;
using Xunit;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Dapper;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale
{
    [Collection("ApiCollection")]
    public class DisablePointOfSaleTest : BaseFunctionalTest
    {
        public DisablePointOfSaleTest(FunctionalTestWebAppFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task DisablePointOfSale_WithValidActivePointOfSale_ShouldReturnNoContentAndSetInactive()
        {
            // Arrange
            var phoneNumber = "+59891234567";
            var posId = await CreatePointOfSaleAsync(phoneNumber);

            // Verify the point of sale is initially active
            var initialState = await GetPointOfSaleIsActiveAsync(posId);
            initialState.Should().BeTrue();

            // Act
            var response = await HttpClient.PatchAsync($"api/pos/{phoneNumber}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            
            // Verify the point of sale is now inactive in the database
            var finalState = await GetPointOfSaleIsActiveAsync(posId);
            finalState.Should().BeFalse();
        }

        [Fact]
        public async Task DisablePointOfSale_WithNonExistentPointOfSale_ShouldReturnBadRequest()
        {
            // Arrange
            var nonExistentPhoneNumber = "+59899999999";

            // Act
            var response = await HttpClient.PatchAsync($"api/pos/{nonExistentPhoneNumber}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DisablePointOfSale_WithAlreadyDisabledPointOfSale_ShouldReturnBadRequest()
        {
            // Arrange
            var phoneNumber = "+59891234568";
            var posId = await CreatePointOfSaleAsync(phoneNumber);

            // First disable the point of sale
            var firstResponse = await HttpClient.PatchAsync($"api/pos/{phoneNumber}", null);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify it's disabled
            var disabledState = await GetPointOfSaleIsActiveAsync(posId);
            disabledState.Should().BeFalse();

            // Act - Try to disable again
            var secondResponse = await HttpClient.PatchAsync($"api/pos/{phoneNumber}", null);

            // Assert
            secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<bool> GetPointOfSaleIsActiveAsync(Guid posId)
        {
            const string sql = "SELECT is_active FROM point_of_sale WHERE id = @Id";
            
            using var connection = SqlConnectionFactory.CreateConnection();
            var isActive = await connection.QuerySingleAsync<bool>(sql, new { Id = posId });
            
            return isActive;
        }
    }
}