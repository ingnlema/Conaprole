using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using Conaprole.Orders.Api.Controllers.Distributors.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Api.FunctionalTests.Distributors
{
    [Collection("ApiCollection")]
    public class CreateDistributorTest : BaseFunctionalTest
    {
        public CreateDistributorTest(FunctionalTestWebAppFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task CreateDistributor_WithValidData_ShouldReturnCreatedAndGuid()
        {
            // Arrange
            var request = new CreateDistributorRequest(
                "Distribuidor Test",
                "+59899123456",
                "Calle Test 123",
                new List<string> { "LACTEOS", "CONGELADOS" }
            );

            // Act
            var response = await HttpClient.PostAsJsonAsync("api/distributors", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var distributorId = await response.Content.ReadFromJsonAsync<Guid>();
            distributorId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateDistributor_WithDuplicatePhoneNumber_ShouldReturnBadRequest()
        {
            // Arrange
            var phoneNumber = "+59899987654";
            
            // Create first distributor
            var firstRequest = new CreateDistributorRequest(
                "Primer Distribuidor",
                phoneNumber,
                "Calle Primera 123",
                new List<string> { "LACTEOS" }
            );
            
            var firstResponse = await HttpClient.PostAsJsonAsync("api/distributors", firstRequest);
            firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Try to create second distributor with same phone number
            var secondRequest = new CreateDistributorRequest(
                "Segundo Distribuidor",
                phoneNumber,
                "Calle Segunda 456",
                new List<string> { "CONGELADOS" }
            );

            // Act
            var secondResponse = await HttpClient.PostAsJsonAsync("api/distributors", secondRequest);

            // Assert
            secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}