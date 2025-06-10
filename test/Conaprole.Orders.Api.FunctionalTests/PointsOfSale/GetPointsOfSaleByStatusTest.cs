using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale
{
    [Collection("ApiCollection")]
    public class GetPointsOfSaleByStatusTest : BaseFunctionalTest
    {
        public GetPointsOfSaleByStatusTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetPointsOfSale_WithStatusActive_ShouldReturnOnlyActivePointsOfSale()
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
            var response = await HttpClient.GetAsync("api/pos?status=active");

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
        }

        [Fact]
        public async Task GetPointsOfSale_WithStatusInactive_ShouldReturnOnlyInactivePointsOfSale()
        {
            // Arrange
            var activePhoneNumber1 = "+59891111121";
            var activePhoneNumber2 = "+59891111122";
            var inactivePhoneNumber1 = "+59891111123";
            var inactivePhoneNumber2 = "+59891111124";

            // Create both active and inactive points of sale
            var activeId1 = await CreatePointOfSaleAsync(activePhoneNumber1);
            var activeId2 = await CreatePointOfSaleAsync(activePhoneNumber2);
            var inactiveId1 = await CreateInactivePointOfSaleAsync(inactivePhoneNumber1);
            var inactiveId2 = await CreateInactivePointOfSaleAsync(inactivePhoneNumber2);

            // Act
            var response = await HttpClient.GetAsync("api/pos?status=inactive");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pointsOfSale = await response.Content.ReadFromJsonAsync<List<PointOfSaleResponse>>();
            pointsOfSale.Should().NotBeNull();
            
            // Verify only inactive points of sale are returned
            pointsOfSale.Should().OnlyContain(pos => pos.IsActive == false, 
                "only inactive points of sale should be returned");
            
            // Verify the inactive points we created are included
            pointsOfSale.Should().Contain(pos => pos.Id == inactiveId1, 
                "first inactive point of sale should be included");
            pointsOfSale.Should().Contain(pos => pos.Id == inactiveId2, 
                "second inactive point of sale should be included");
            
            // Verify the active points are not included
            pointsOfSale.Should().NotContain(pos => pos.Id == activeId1, 
                "first active point of sale should not be included");
            pointsOfSale.Should().NotContain(pos => pos.Id == activeId2, 
                "second active point of sale should not be included");
        }

        [Fact]
        public async Task GetPointsOfSale_WithStatusAll_ShouldReturnAllPointsOfSale()
        {
            // Arrange
            var activePhoneNumber1 = "+59891111131";
            var activePhoneNumber2 = "+59891111132";
            var inactivePhoneNumber1 = "+59891111133";
            var inactivePhoneNumber2 = "+59891111134";

            // Create both active and inactive points of sale
            var activeId1 = await CreatePointOfSaleAsync(activePhoneNumber1);
            var activeId2 = await CreatePointOfSaleAsync(activePhoneNumber2);
            var inactiveId1 = await CreateInactivePointOfSaleAsync(inactivePhoneNumber1);
            var inactiveId2 = await CreateInactivePointOfSaleAsync(inactivePhoneNumber2);

            // Act
            var response = await HttpClient.GetAsync("api/pos?status=all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pointsOfSale = await response.Content.ReadFromJsonAsync<List<PointOfSaleResponse>>();
            pointsOfSale.Should().NotBeNull();
            
            // Verify all points of sale are returned (both active and inactive)
            pointsOfSale.Should().Contain(pos => pos.Id == activeId1, 
                "first active point of sale should be included");
            pointsOfSale.Should().Contain(pos => pos.Id == activeId2, 
                "second active point of sale should be included");
            pointsOfSale.Should().Contain(pos => pos.Id == inactiveId1, 
                "first inactive point of sale should be included");
            pointsOfSale.Should().Contain(pos => pos.Id == inactiveId2, 
                "second inactive point of sale should be included");
            
            // Verify we have both active and inactive POS
            pointsOfSale.Should().Contain(pos => pos.IsActive == true, 
                "should contain at least one active point of sale");
            pointsOfSale.Should().Contain(pos => pos.IsActive == false, 
                "should contain at least one inactive point of sale");
        }

        [Fact]
        public async Task GetPointsOfSale_WithoutStatusParameter_ShouldDefaultToActive()
        {
            // Arrange
            var activePhoneNumber = "+59891111141";
            var inactivePhoneNumber = "+59891111142";

            // Create both active and inactive points of sale
            var activeId = await CreatePointOfSaleAsync(activePhoneNumber);
            var inactiveId = await CreateInactivePointOfSaleAsync(inactivePhoneNumber);

            // Act
            var response = await HttpClient.GetAsync("api/pos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pointsOfSale = await response.Content.ReadFromJsonAsync<List<PointOfSaleResponse>>();
            pointsOfSale.Should().NotBeNull();
            
            // Verify only active points of sale are returned (default behavior)
            pointsOfSale.Should().OnlyContain(pos => pos.IsActive == true, 
                "only active points of sale should be returned by default");
            
            // Verify the active point we created is included
            pointsOfSale.Should().Contain(pos => pos.Id == activeId, 
                "active point of sale should be included");
            
            // Verify the inactive point is not included
            pointsOfSale.Should().NotContain(pos => pos.Id == inactiveId, 
                "inactive point of sale should not be included");
        }

        [Fact]
        public async Task GetPointsOfSale_WithInvalidStatus_ShouldDefaultToActive()
        {
            // Arrange
            var activePhoneNumber = "+59891111151";
            var inactivePhoneNumber = "+59891111152";

            // Create both active and inactive points of sale
            var activeId = await CreatePointOfSaleAsync(activePhoneNumber);
            var inactiveId = await CreateInactivePointOfSaleAsync(inactivePhoneNumber);

            // Act
            var response = await HttpClient.GetAsync("api/pos?status=invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pointsOfSale = await response.Content.ReadFromJsonAsync<List<PointOfSaleResponse>>();
            pointsOfSale.Should().NotBeNull();
            
            // Verify only active points of sale are returned (fallback to default)
            pointsOfSale.Should().OnlyContain(pos => pos.IsActive == true, 
                "only active points of sale should be returned for invalid status");
            
            // Verify the active point we created is included
            pointsOfSale.Should().Contain(pos => pos.Id == activeId, 
                "active point of sale should be included");
            
            // Verify the inactive point is not included
            pointsOfSale.Should().NotContain(pos => pos.Id == inactiveId, 
                "inactive point of sale should not be included");
        }
    }
}