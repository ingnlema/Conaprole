using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale;

[Collection("IntegrationCollection")]
public class CreatePointOfSaleTest : BaseIntegrationTest
{
    public CreatePointOfSaleTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreatePointOfSale_WithValidData_ShouldReturnSuccessAndValidId()
    {
        // Arrange - Create a unique phone number for this test to avoid conflicts
        var command = new CreatePointOfSaleCommand(
            "Test POS Valid",
            "+59899999999", // Unique phone number for this test
            "Montevideo",
            "Test Street 123",
            "11000"
        );

        // Act - Send the command using the inherited Sender
        var result = await Sender.Send(command);

        // Assert - Verify the result is successful
        Assert.False(result.IsFailure);
        Assert.True(result.Value != Guid.Empty);

        // Additional verification: ensure data matches what was sent
        // Since we don't have a "Get" operation, we can verify the ID is returned
        var pointOfSaleId = result.Value;
        Assert.NotEqual(Guid.Empty, pointOfSaleId);
    }

    [Fact]
    public async Task CreatePointOfSale_WithDuplicatePhoneNumber_ShouldReturnFailure()
    {
        // Arrange - First, create a point of sale
        var firstPointOfSaleId = await PointOfSaleData.SeedAsync(Sender);
        Assert.NotEqual(Guid.Empty, firstPointOfSaleId);

        // Try to create another point of sale with the same phone number
        var duplicateCommand = PointOfSaleData.CreateCommand;

        // Act - Attempt to create duplicate
        var result = await Sender.Send(duplicateCommand);

        // Assert - Should fail due to duplicate phone number
        Assert.True(result.IsFailure);
        Assert.Equal("PointOfSale.AlreadyExists", result.Error.Code);
    }
}