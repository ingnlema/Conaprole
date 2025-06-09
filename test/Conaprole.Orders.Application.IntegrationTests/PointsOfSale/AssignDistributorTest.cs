using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale;

[Collection("IntegrationCollection")]
public class AssignDistributorTest : BaseIntegrationTest
{
    public AssignDistributorTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task AssignDistributor_WithValidData_ShouldReturnSuccess()
    {
        // Arrange - Use unique phone numbers to avoid test conflicts
        var posPhoneNumber = "+59891000001";
        var distributorPhoneNumber = "+59891000002";
        var category = Category.LACTEOS;

        // Create point of sale
        var createPosCommand = new CreatePointOfSaleCommand(
            "Punto Test Assign Success",
            posPhoneNumber,
            "Montevideo",
            "Calle Test 001",
            "11000"
        );
        var posResult = await Sender.Send(createPosCommand);
        Assert.False(posResult.IsFailure);

        // Create distributor with the required category
        var createDistributorCommand = new CreateDistributorCommand(
            "Distribuidor Test Assign Success",
            distributorPhoneNumber,
            "Dirección Test 001",
            new List<Category> { category }
        );
        var distributorResult = await Sender.Send(createDistributorCommand);
        Assert.False(distributorResult.IsFailure);

        // Act - Assign distributor to point of sale
        var assignCommand = new AssignDistributorToPointOfSaleCommand(
            posPhoneNumber,
            distributorPhoneNumber,
            category
        );
        var assignResult = await Sender.Send(assignCommand);

        // Assert
        Assert.False(assignResult.IsFailure);
        Assert.True(assignResult.Value);
    }

    [Fact]
    public async Task AssignDistributor_WhenPointOfSaleNotFound_ShouldReturnFailure()
    {
        // Arrange - Create distributor but not point of sale
        var distributorPhoneNumber = "+59891000012";
        var category = Category.LACTEOS;

        var createDistributorCommand = new CreateDistributorCommand(
            "Distributor Test POSNotFound",
            distributorPhoneNumber,
            "Dirección Test 012",
            new List<Category> { category }
        );
        var distributorResult = await Sender.Send(createDistributorCommand);
        Assert.False(distributorResult.IsFailure);

        var assignCommand = new AssignDistributorToPointOfSaleCommand(
            "+59891000999", // Non-existent point of sale
            distributorPhoneNumber,
            category
        );

        // Act
        var result = await Sender.Send(assignCommand);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task AssignDistributor_WhenDistributorNotFound_ShouldReturnFailure()
    {
        // Arrange - Create point of sale but not distributor
        var posPhoneNumber = "+59891000021";

        var createPosCommand = new CreatePointOfSaleCommand(
            "Punto Test DistributorNotFound",
            posPhoneNumber,
            "Montevideo",
            "Calle Test 021",
            "11000"
        );
        var posResult = await Sender.Send(createPosCommand);
        Assert.False(posResult.IsFailure);

        var assignCommand = new AssignDistributorToPointOfSaleCommand(
            posPhoneNumber,
            "+59891000998", // Non-existent distributor
            Category.LACTEOS
        );

        // Act
        var result = await Sender.Send(assignCommand);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task AssignDistributor_WhenCategoryNotSupported_ShouldReturnFailure()
    {
        // Arrange - Create entities where distributor doesn't support the requested category
        var posPhoneNumber = "+59891000031";
        var distributorPhoneNumber = "+59891000032";

        // Create point of sale
        var createPosCommand = new CreatePointOfSaleCommand(
            "Punto Test CategoryNotSupported",
            posPhoneNumber,
            "Montevideo",
            "Calle Test 031",
            "11000"
        );
        var posResult = await Sender.Send(createPosCommand);
        Assert.False(posResult.IsFailure);

        // Create distributor that only supports LACTEOS
        var createDistributorCommand = new CreateDistributorCommand(
            "Distributor Test CategoryNotSupported",
            distributorPhoneNumber,
            "Dirección Test 031",
            new List<Category> { Category.LACTEOS } // Only supports LACTEOS
        );
        var distributorResult = await Sender.Send(createDistributorCommand);
        Assert.False(distributorResult.IsFailure);

        // Try to assign for CONGELADOS category (not supported)
        var assignCommand = new AssignDistributorToPointOfSaleCommand(
            posPhoneNumber,
            distributorPhoneNumber,
            Category.CONGELADOS // Not supported by distributor
        );

        // Act
        var result = await Sender.Send(assignCommand);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task AssignDistributor_WhenAlreadyAssigned_ShouldReturnFailure()
    {
        // Arrange - Create and assign entities, then try to assign again
        var posPhoneNumber = "+59891000041";
        var distributorPhoneNumber = "+59891000042";
        var category = Category.LACTEOS;

        // Create point of sale
        var createPosCommand = new CreatePointOfSaleCommand(
            "Punto Test AlreadyAssigned",
            posPhoneNumber,
            "Montevideo",
            "Calle Test 041",
            "11000"
        );
        var posResult = await Sender.Send(createPosCommand);
        Assert.False(posResult.IsFailure);

        // Create distributor
        var createDistributorCommand = new CreateDistributorCommand(
            "Distributor Test AlreadyAssigned",
            distributorPhoneNumber,
            "Dirección Test 041",
            new List<Category> { category }
        );
        var distributorResult = await Sender.Send(createDistributorCommand);
        Assert.False(distributorResult.IsFailure);

        // First assignment - should succeed
        var firstAssignCommand = new AssignDistributorToPointOfSaleCommand(
            posPhoneNumber,
            distributorPhoneNumber,
            category
        );
        var firstAssignResult = await Sender.Send(firstAssignCommand);
        Assert.False(firstAssignResult.IsFailure);
        Assert.True(firstAssignResult.Value);

        // Act - Try to assign again (should fail)
        var secondAssignCommand = new AssignDistributorToPointOfSaleCommand(
            posPhoneNumber,
            distributorPhoneNumber,
            category
        );
        var secondAssignResult = await Sender.Send(secondAssignCommand);

        // Assert
        Assert.True(secondAssignResult.IsFailure);
    }
}