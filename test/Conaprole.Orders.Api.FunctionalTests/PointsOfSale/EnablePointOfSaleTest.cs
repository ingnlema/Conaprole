using System.Net;
using FluentAssertions;
using Xunit;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Dapper;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale
{
    [Collection("ApiCollection")]
    public class EnablePointOfSaleTest : BaseFunctionalTest
    {
        public EnablePointOfSaleTest(FunctionalTestWebAppFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task EnablePointOfSale_WithValidInactivePointOfSale_ShouldReturnNoContentAndSetActive()
        {
            // Arrange
            var phoneNumber = "+59891234569";
            var posId = await CreateInactivePointOfSaleAsync(phoneNumber);

            // Verify the point of sale is initially inactive
            var initialState = await GetPointOfSaleIsActiveAsync(posId);
            initialState.Should().BeFalse();

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.PatchAsync($"api/pos/{phoneNumber}/enable", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            
            // Verify the point of sale is now active in the database
            var finalState = await GetPointOfSaleIsActiveAsync(posId);
            finalState.Should().BeTrue();
        }

        [Fact]
        public async Task EnablePointOfSale_WithNonExistentPointOfSale_ShouldReturnBadRequest()
        {
            // Arrange
            var nonExistentPhoneNumber = "+59899999998";

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.PatchAsync($"api/pos/{nonExistentPhoneNumber}/enable", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task EnablePointOfSale_WithAlreadyEnabledPointOfSale_ShouldReturnBadRequest()
        {
            // Arrange
            var phoneNumber = "+59891234570";
            var posId = await CreatePointOfSaleAsync(phoneNumber);

            // Verify the point of sale is initially active
            var initialState = await GetPointOfSaleIsActiveAsync(posId);
            initialState.Should().BeTrue();

            // Act - Try to enable an already active point of sale
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.PatchAsync($"api/pos/{phoneNumber}/enable", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task EnablePointOfSale_WithDisabledPointOfSale_ShouldEnableSuccessfully()
        {
            // Arrange
            var phoneNumber = "+59891234571";
            var posId = await CreatePointOfSaleAsync(phoneNumber);

            // First disable the point of sale
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var disableResponse = await HttpClient.PatchAsync($"api/pos/{phoneNumber}", null);
            disableResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify it's disabled
            var disabledState = await GetPointOfSaleIsActiveAsync(posId);
            disabledState.Should().BeFalse();

            // Act - Enable the disabled point of sale
            var enableResponse = await HttpClient.PatchAsync($"api/pos/{phoneNumber}/enable", null);

            // Assert
            enableResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            
            // Verify the point of sale is now active
            var finalState = await GetPointOfSaleIsActiveAsync(posId);
            finalState.Should().BeTrue();
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