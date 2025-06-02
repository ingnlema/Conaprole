using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using Conaprole.Orders.Api.Controllers.Distributors.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;
using Dapper;

namespace Conaprole.Orders.Api.FunctionalTests.Distributors
{
    [Collection("ApiCollection")]
    public class RemoveCategoryTest : BaseFunctionalTest
    {
        public RemoveCategoryTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task RemoveCategory_ExistingCategory_ShouldReturnNoContent()
        {
            // Arrange
            var distributorPhone = "+59890000001";
            
            // Create distributor with multiple categories
            await CreateDistributorWithCategoriesAsync(distributorPhone, Category.LACTEOS, Category.CONGELADOS);
            
            var request = new RemoveDistributorCategoryRequest("LACTEOS");

            // Act
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/distributors/{distributorPhone}/categories")
            {
                Content = JsonContent.Create(request)
            };
            var response = await HttpClient.SendAsync(httpRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            
            // Verify the category was removed from the database
            var categories = await GetDistributorCategoriesFromDatabaseAsync(distributorPhone);
            categories.Should().NotContain("LACTEOS");
            categories.Should().Contain("CONGELADOS");
        }

        [Fact]
        public async Task RemoveCategory_NonExistingCategory_ShouldReturnBadRequest()
        {
            // Arrange
            var distributorPhone = "+59890000002";
            
            // Create distributor with only LACTEOS category
            await CreateDistributorWithCategoriesAsync(distributorPhone, Category.LACTEOS);
            
            var request = new RemoveDistributorCategoryRequest("CONGELADOS");

            // Act
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/distributors/{distributorPhone}/categories")
            {
                Content = JsonContent.Create(request)
            };
            var response = await HttpClient.SendAsync(httpRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RemoveCategory_NonExistingDistributor_ShouldReturnBadRequest()
        {
            // Arrange
            var nonExistingPhone = "+59890000999";
            var request = new RemoveDistributorCategoryRequest("LACTEOS");

            // Act
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/distributors/{nonExistingPhone}/categories")
            {
                Content = JsonContent.Create(request)
            };
            var response = await HttpClient.SendAsync(httpRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task CreateDistributorWithCategoriesAsync(string phoneNumber, params Category[] categories)
        {
            var id = Guid.NewGuid();

            const string sql = @"
            INSERT INTO distributor (id, phone_number, name, address, supported_categories, created_at)
            VALUES (@Id, @PhoneNumber, @Name, @Address, @Categories, now());";

            using var connection = SqlConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                PhoneNumber = phoneNumber,
                Name = "Distribuidor Test",
                Address = "Calle Falsa 123",
                Categories = string.Join(",", categories.Select(c => c.ToString()))
            });
        }

        private async Task<List<string>> GetDistributorCategoriesFromDatabaseAsync(string phoneNumber)
        {
            const string sql = @"
            SELECT supported_categories 
            FROM distributor 
            WHERE phone_number = @PhoneNumber;";

            using var connection = SqlConnectionFactory.CreateConnection();
            var categoriesString = await connection.QuerySingleOrDefaultAsync<string>(sql, new { PhoneNumber = phoneNumber });
            
            if (string.IsNullOrEmpty(categoriesString))
                return new List<string>();
                
            return categoriesString.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}