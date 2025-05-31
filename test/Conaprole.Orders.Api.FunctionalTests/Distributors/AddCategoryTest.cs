using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Distributors.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;
using Dapper;
using FluentAssertions;
using Xunit;

namespace Conaprole.Orders.Api.FunctionalTests.Distributors;

[Collection("ApiCollection")]
public class AddCategoryTest : BaseFunctionalTest
{
    public AddCategoryTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AddCategory_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        var distributorPhone = "+59899887766";
        await CreateDistributorAsync(distributorPhone);
        
        var request = new AddDistributorCategoryRequest("CONGELADOS");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"api/distributors/{distributorPhone}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the category was added to the distributor in the database
        await VerifyDistributorHasCategory(distributorPhone, Category.CONGELADOS);
    }

    [Fact]
    public async Task AddCategory_WithNonExistentDistributor_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentPhone = "+59899999999";
        var request = new AddDistributorCategoryRequest("LACTEOS");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"api/distributors/{nonExistentPhone}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddCategory_WithAlreadyAssignedCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var distributorPhone = "+59899887766";
        await CreateDistributorAsync(distributorPhone); // This already creates a distributor with LACTEOS category
        
        var request = new AddDistributorCategoryRequest("LACTEOS");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"api/distributors/{distributorPhone}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddCategory_WithInvalidCategory_ShouldReturnInternalServerError()
    {
        // Arrange
        var distributorPhone = "+59899887766";
        await CreateDistributorAsync(distributorPhone);
        
        var request = new AddDistributorCategoryRequest("INVALID_CATEGORY");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"api/distributors/{distributorPhone}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    private async Task VerifyDistributorHasCategory(string phoneNumber, Category expectedCategory)
    {
        const string sql = @"
            SELECT supported_categories 
            FROM distributor 
            WHERE phone_number = @PhoneNumber";

        using var connection = SqlConnectionFactory.CreateConnection();
        var supportedCategoriesStr = await connection.QuerySingleOrDefaultAsync<string>(sql, new { PhoneNumber = phoneNumber });
        
        supportedCategoriesStr.Should().NotBeNull();
        var categories = supportedCategoriesStr!.Split(',', StringSplitOptions.RemoveEmptyEntries);
        categories.Should().Contain(expectedCategory.ToString());
    }
}