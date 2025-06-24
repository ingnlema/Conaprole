using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale
{
    [Collection("ApiCollection")]
    public class GetActivePointsOfSaleTest : BaseFunctionalTest
    {
        public GetActivePointsOfSaleTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetActivePointsOfSale_ShouldReturnOnlyActivePointsOfSale()
        {
            // Arrange
            var activePhoneNumber1 = "+59891111111";
            var activePhoneNumber2 = "+59891111112";
            var inactivePhoneNumber = "+59891111113";

            // Create both active and inactive points of sale
            var activeId1 = await CreatePointOfSaleAsync(activePhoneNumber1);
            var activeId2 = await CreatePointOfSaleAsync(activePhoneNumber2);
            var inactiveId = await CreateInactivePointOfSaleAsync(inactivePhoneNumber);

            // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var response = await HttpClient.GetAsync("api/pos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pointsOfSale = await response.Content.ReadFromJsonAsync<List<PointOfSaleResponse>>();
            pointsOfSale.Should().NotBeNull();
            
            // Verify only active points of sale are returned
            pointsOfSale.Should().OnlyContain(pos => pos.IsActive == true, 
                "only active points of sale should be returned");
            
            // Verify the active points we created are included
            pointsOfSale.Should().Contain(pos => pos.Id == activeId1, 
                "first active point of sale should be included");
            pointsOfSale.Should().Contain(pos => pos.Id == activeId2, 
                "second active point of sale should be included");
            
            // Verify the inactive point is not included
            pointsOfSale.Should().NotContain(pos => pos.Id == inactiveId, 
                "inactive point of sale should not be included");

            // Verify response structure
            foreach (var pos in pointsOfSale)
            {
                pos.Id.Should().NotBeEmpty();
                pos.Name.Should().NotBeNullOrEmpty();
                pos.PhoneNumber.Should().NotBeNullOrEmpty();
                pos.Address.Should().NotBeNull();
                pos.Address.City.Should().NotBeNullOrEmpty();
                pos.Address.Street.Should().NotBeNullOrEmpty();
                pos.Address.ZipCode.Should().NotBeNullOrEmpty();
                pos.IsActive.Should().BeTrue();
                pos.CreatedAt.Should().NotBe(default(DateTime));
            }
        }
    }
}