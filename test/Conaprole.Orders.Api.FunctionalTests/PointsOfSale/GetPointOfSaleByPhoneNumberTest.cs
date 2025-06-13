using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale
{
    [Collection("ApiCollection")]
    public class GetPointOfSaleByPhoneNumberTest : BaseFunctionalTest
    {
        public GetPointOfSaleByPhoneNumberTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetPointOfSaleByPhoneNumber_Should_Return_PointOfSale_From_Legacy_Route()
        {
            // Arrange
            var phoneNumber = "+59891234567";
            var posId = await CreatePointOfSaleAsync(phoneNumber);

            // Act
            var response = await HttpClient.GetAsync($"api/pos/by-phone/{phoneNumber}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var pointOfSale = await response.Content.ReadFromJsonAsync<PointOfSaleResponse>();
            pointOfSale.Should().NotBeNull();
            pointOfSale!.Id.Should().Be(posId);
            pointOfSale.PhoneNumber.Should().Be(phoneNumber);
            pointOfSale.IsActive.Should().BeTrue();
        }
        
        [Fact]
        public async Task GetPointOfSaleByPhoneNumber_Should_Return_NotFound_When_PhoneNumber_DoesNotExist()
        {
            // Arrange
            var nonExistentPhoneNumber = "+59899999999";

            // Act
            var response1 = await HttpClient.GetAsync($"api/pos/by-phone/{nonExistentPhoneNumber}");

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPointOfSaleByPhoneNumber_Should_Return_InactivePointOfSale()
        {
            // Arrange
            var phoneNumber = "+59891234569";
            var posId = await CreateInactivePointOfSaleAsync(phoneNumber);

            // Act
            var response = await HttpClient.GetAsync($"api/pos/by-phone/{phoneNumber}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var pointOfSale = await response.Content.ReadFromJsonAsync<PointOfSaleResponse>();
            pointOfSale.Should().NotBeNull();
            pointOfSale!.Id.Should().Be(posId);
            pointOfSale.PhoneNumber.Should().Be(phoneNumber);
            pointOfSale.IsActive.Should().BeFalse();
        }
    }
}