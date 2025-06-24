using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale;

[Collection("ApiCollection")]
public class CreatePointOfSaleTest : BaseFunctionalTest
{
    public CreatePointOfSaleTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreatePointOfSaleRequest(
            "Test POS",
            "+59891234567",
            "Montevideo",
            "Test Street 123",
            "11200"
        );

        // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

        var response = await HttpClient.PostAsJsonAsync("/api/pos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenPhoneNumberAlreadyExists()
    {
        // Arrange
        var phoneNumber = "+59891234568";
        await CreatePointOfSaleAsync(phoneNumber);
        
        var request = new CreatePointOfSaleRequest(
            "Another POS",
            phoneNumber,
            "Montevideo", 
            "Another Street 456",
            "11300"
        );

        // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

        var response = await HttpClient.PostAsJsonAsync("/api/pos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenRequiredFieldsAreMissing()
    {
        // Arrange
        var request = new CreatePointOfSaleRequest(
            "", // Empty name
            "+59891234569",
            "Montevideo",
            "Test Street 789",
            "11400"
        );

        // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

        var response = await HttpClient.PostAsJsonAsync("/api/pos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}