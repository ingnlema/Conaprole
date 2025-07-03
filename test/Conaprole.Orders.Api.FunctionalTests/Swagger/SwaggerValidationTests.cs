using System.Text.Json;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;

namespace Conaprole.Orders.Api.FunctionalTests.Swagger;

/// <summary>
/// Tests to validate OpenAPI/Swagger specification generation
/// </summary>
public class SwaggerValidationTests : BaseFunctionalTest
{
    public SwaggerValidationTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SwaggerEndpoint_ShouldReturnValidOpenApiDocument()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content));

        // Validate it's valid JSON
        var jsonDocument = JsonDocument.Parse(content);
        Assert.NotNull(jsonDocument);
    }

    [Fact]
    public async Task OpenApiDocument_ShouldContainRequiredInformation()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        // Parse as JSON to validate structure
        var jsonDocument = JsonDocument.Parse(content);
        var root = jsonDocument.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("info", out var info));
        Assert.True(info.TryGetProperty("title", out var title));
        Assert.Equal("Conaprole Orders API", title.GetString());
        
        Assert.True(info.TryGetProperty("version", out var version));
        Assert.Equal("v1.1", version.GetString());
        
        Assert.True(info.TryGetProperty("description", out var description));
        Assert.Contains("dairy product orders", description.GetString());

        // Verify that we have paths defined
        Assert.True(root.TryGetProperty("paths", out var paths));
        Assert.True(paths.EnumerateObject().Any());

        // We should have at least some API endpoints documented (even if some require authentication)
        var pathNames = paths.EnumerateObject().Select(p => p.Name).ToList();
        Assert.NotEmpty(pathNames);
    }

    [Fact]
    public async Task OpenApiDocument_ShouldContainTagsForOrganization()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        // Parse as JSON to validate structure
        var jsonDocument = JsonDocument.Parse(content);
        var root = jsonDocument.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("paths", out var paths));
        
        // Check that we have operations with tags for better organization
        var foundTags = false;
        foreach (var pathItem in paths.EnumerateObject())
        {
            foreach (var operation in pathItem.Value.EnumerateObject())
            {
                if (operation.Value.TryGetProperty("tags", out var tags))
                {
                    if (tags.EnumerateArray().Any())
                    {
                        foundTags = true;
                        break;
                    }
                }
                if (foundTags) break;
            }
            if (foundTags) break;
        }

        Assert.True(foundTags, "Should have tags for API organization");
    }

    [Fact]
    public async Task SwaggerUI_ShouldBeAccessible()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/swagger");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        
        var content = await response.Content.ReadAsStringAsync();
        // Just check that we get some HTML content back (Swagger UI is working)
        Assert.Contains("<html", content);
        Assert.Contains("swagger", content.ToLower());
    }
}