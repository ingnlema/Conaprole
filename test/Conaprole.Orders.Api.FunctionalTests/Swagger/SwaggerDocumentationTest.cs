using System.Net;
using FluentAssertions;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;

namespace Conaprole.Orders.Api.FunctionalTests.Swagger;

[Collection("ApiCollection")]
public class SwaggerDocumentationTest : BaseFunctionalTest
{
    public SwaggerDocumentationTest(FunctionalTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task SwaggerDocument_ShouldBeAccessible()
    {
        // Act
        var response = await HttpClient.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("application/json");
    }

    [Fact]
    public async Task SwaggerDocument_ShouldContainAllControllers()
    {
        // Act
        var response = await HttpClient.GetAsync("/swagger/v1/swagger.json");
        var jsonContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the document contains all controller paths
        jsonContent.Should().Contain("/api/users");
        jsonContent.Should().Contain("/api/products");
        jsonContent.Should().Contain("/api/orders");
        jsonContent.Should().Contain("/api/distributors");
        jsonContent.Should().Contain("/api/pos");
    }

    [Fact]
    public async Task SwaggerDocument_ShouldContainSwaggerOperationSummaries()
    {
        // Act
        var response = await HttpClient.GetAsync("/swagger/v1/swagger.json");
        var jsonContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify the document contains operation summaries
        jsonContent.Should().Contain("Get logged in user");
        jsonContent.Should().Contain("Register new user");
        jsonContent.Should().Contain("User login");
        jsonContent.Should().Contain("Get all products");
        jsonContent.Should().Contain("Create order");
        jsonContent.Should().Contain("Create a new distributor");
        jsonContent.Should().Contain("List all active POS");
    }

    [Fact]
    public async Task SwaggerUI_ShouldBeAccessible()
    {
        // Act
        var response = await HttpClient.GetAsync("/swagger");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("text/html");
    }
}