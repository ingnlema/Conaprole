using System.Net;
using FluentAssertions;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.FunctionalTests.PointsOfSale;

[Collection("ApiCollection")]
public class UnassignDistributorTest : BaseFunctionalTest
{
    public UnassignDistributorTest(FunctionalTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task UnassignDistributor_WhenAssignmentExists_ShouldReturnNoContent()
    {
        // Arrange - Use unique phone numbers to avoid test conflicts
        var posPhoneNumber = "+59891000001";
        var distributorPhoneNumber = "+59891000002";
        var category = Category.LACTEOS;

        var posId = await CreatePointOfSaleAsync(posPhoneNumber);
        var distributorId = await CreateDistributorAsync(distributorPhoneNumber);
        await AssignDistributorToPointOfSaleAsync(posId, distributorId, category);

        // Verify assignment exists before test
        var isAssignedBefore = await IsDistributorAssignedAsync(posId, distributorId, category);
        isAssignedBefore.Should().BeTrue();

        // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

        var response = await HttpClient.DeleteAsync($"api/pos/{posPhoneNumber}/distributors/{distributorPhoneNumber}/categories/{category}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify assignment was removed from database
        var isAssignedAfter = await IsDistributorAssignedAsync(posId, distributorId, category);
        isAssignedAfter.Should().BeFalse();
    }

    [Fact]
    public async Task UnassignDistributor_WhenPointOfSaleNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentPosPhoneNumber = "+59891000003";
        var distributorPhoneNumber = "+59891000004";
        var category = Category.LACTEOS;

        await CreateDistributorAsync(distributorPhoneNumber);

        // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

        var response = await HttpClient.DeleteAsync($"api/pos/{nonExistentPosPhoneNumber}/distributors/{distributorPhoneNumber}/categories/{category}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UnassignDistributor_WhenDistributorNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var posPhoneNumber = "+59891000005";
        var nonExistentDistributorPhoneNumber = "+59891000006";
        var category = Category.LACTEOS;

        await CreatePointOfSaleAsync(posPhoneNumber);

        // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

        var response = await HttpClient.DeleteAsync($"api/pos/{posPhoneNumber}/distributors/{nonExistentDistributorPhoneNumber}/categories/{category}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UnassignDistributor_WhenAssignmentNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var posPhoneNumber = "+59891000007";
        var distributorPhoneNumber = "+59891000008";
        var category = Category.LACTEOS;

        await CreatePointOfSaleAsync(posPhoneNumber);
        await CreateDistributorAsync(distributorPhoneNumber);
        // Note: We don't create an assignment, so it doesn't exist

        // Act
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

        var response = await HttpClient.DeleteAsync($"api/pos/{posPhoneNumber}/distributors/{distributorPhoneNumber}/categories/{category}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}