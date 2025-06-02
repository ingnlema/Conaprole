using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.FunctionalTests.Distributors
{
    [Collection("ApiCollection")]
    public class GetCategorieTest : BaseFunctionalTest
    {
        public GetCategorieTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetCategories_WithValidDistributor_ShouldReturnCategories()
        {
            // Arrange
            var distributorPhone = "+59890000000";
            await CreateDistributorAsync(distributorPhone);

            // Act
            var response = await HttpClient.GetAsync($"api/distributors/{distributorPhone}/categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var categories = await response.Content.ReadFromJsonAsync<List<string>>();
            categories.Should().NotBeNull();
            categories.Should().NotBeEmpty();
            categories.Should().Contain(Category.LACTEOS.ToString());
        }

        [Fact]
        public async Task GetCategories_WithNonExistentDistributor_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistentPhone = "+59800000001";

            // Act
            var response = await HttpClient.GetAsync($"api/distributors/{nonExistentPhone}/categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var categories = await response.Content.ReadFromJsonAsync<List<string>>();
            categories.Should().NotBeNull();
            categories.Should().BeEmpty();
        }
    }
}