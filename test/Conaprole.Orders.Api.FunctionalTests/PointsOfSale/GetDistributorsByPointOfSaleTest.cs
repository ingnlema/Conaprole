using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.PointsOfSale.GetDistributorsByPointOfSale;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale
{
    [Collection("ApiCollection")]
    public class GetDistributorsByPointOfSaleTest : BaseFunctionalTest
    {
        public GetDistributorsByPointOfSaleTest(FunctionalTestWebAppFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetDistributorsByPointOfSale_WithMultipleDistributors_ShouldReturnDistributorsWithCategories()
        {
            // Arrange
            var posPhoneNumber = "+59891234567";
            var distributor1Phone = "+59899887766";
            var distributor2Phone = "+59899887777";
            var distributor3Phone = "+59899887788";

            // Create point of sale
            var posId = await CreatePointOfSaleAsync(posPhoneNumber);

            // Create distributors
            var distributor1Id = await CreateDistributorAsync(distributor1Phone);
            var distributor2Id = await CreateDistributorAsync(distributor2Phone);
            var distributor3Id = await CreateDistributorAsync(distributor3Phone);

            // Assign distributors to point of sale with different categories
            await AssignDistributorToPointOfSaleAsync(posId, distributor1Id, Category.LACTEOS);
            await AssignDistributorToPointOfSaleAsync(posId, distributor1Id, Category.CONGELADOS);
            await AssignDistributorToPointOfSaleAsync(posId, distributor2Id, Category.LACTEOS);
            await AssignDistributorToPointOfSaleAsync(posId, distributor3Id, Category.SUBPRODUCTOS);

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.GetAsync($"api/pos/{posPhoneNumber}/distributors");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var distributors = await response.Content.ReadFromJsonAsync<List<DistributorWithCategoriesResponse>>();
            distributors.Should().NotBeNull();
            distributors!.Should().HaveCount(3);

            // Verify distributor1 has both LACTEOS and CONGELADOS categories
            var distributor1 = distributors.FirstOrDefault(d => d.PhoneNumber == distributor1Phone);
            distributor1.Should().NotBeNull();
            distributor1!.Name.Should().Be("Distribuidor Test");
            distributor1.Categories.Should().Contain("LACTEOS");
            distributor1.Categories.Should().Contain("CONGELADOS");
            distributor1.Categories.Should().HaveCount(2);

            // Verify distributor2 has only LACTEOS category
            var distributor2 = distributors.FirstOrDefault(d => d.PhoneNumber == distributor2Phone);
            distributor2.Should().NotBeNull();
            distributor2!.Name.Should().Be("Distribuidor Test");
            distributor2.Categories.Should().Contain("LACTEOS");
            distributor2.Categories.Should().HaveCount(1);

            // Verify distributor3 has only SUBPRODUCTOS category
            var distributor3 = distributors.FirstOrDefault(d => d.PhoneNumber == distributor3Phone);
            distributor3.Should().NotBeNull();
            distributor3!.Name.Should().Be("Distribuidor Test");
            distributor3.Categories.Should().Contain("SUBPRODUCTOS");
            distributor3.Categories.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetDistributorsByPointOfSale_WithNoDistributors_ShouldReturnEmptyList()
        {
            // Arrange
            var posPhoneNumber = "+59891234568";
            await CreatePointOfSaleAsync(posPhoneNumber);

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.GetAsync($"api/pos/{posPhoneNumber}/distributors");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var distributors = await response.Content.ReadFromJsonAsync<List<DistributorWithCategoriesResponse>>();
            distributors.Should().NotBeNull();
            distributors!.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDistributorsByPointOfSale_WithNonExistentPointOfSale_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistentPhoneNumber = "+59899999999";

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.GetAsync($"api/pos/{nonExistentPhoneNumber}/distributors");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var distributors = await response.Content.ReadFromJsonAsync<List<DistributorWithCategoriesResponse>>();
            distributors.Should().NotBeNull();
            distributors!.Should().BeEmpty();
        }
    }
}