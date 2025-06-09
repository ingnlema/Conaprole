using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.UnassignDistributor;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale;

[Collection("IntegrationCollection")]
public class UnassignDistributorTest : BaseIntegrationTest
{
    public UnassignDistributorTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task UnassignDistributor_WhenAssignmentExists_ShouldReturnSuccess()
    {
        // Arrange - Use unique phone numbers to avoid test conflicts
        var posPhoneNumber = "+59891000101";
        var distributorPhoneNumber = "+59891000102";
        var category = Category.LACTEOS;

        // Create point of sale
        var createPosCommand = new CreatePointOfSaleCommand(
            "Punto Test Unassign",
            posPhoneNumber,
            "Montevideo",
            "Calle Test 101",
            "11000"
        );
        var posResult = await Sender.Send(createPosCommand);
        Assert.False(posResult.IsFailure);
        var posId = posResult.Value;

        // Create distributor
        var createDistributorCommand = new CreateDistributorCommand(
            "Distribuidor Test Unassign",
            distributorPhoneNumber,
            "Dirección Test 101",
            new List<Category> { category }
        );
        var distributorResult = await Sender.Send(createDistributorCommand);
        Assert.False(distributorResult.IsFailure);
        var distributorId = distributorResult.Value;

        // Assign distributor to point of sale
        var assignCommand = new AssignDistributorToPointOfSaleCommand(
            posPhoneNumber,
            distributorPhoneNumber,
            category
        );
        var assignResult = await Sender.Send(assignCommand);
        Assert.False(assignResult.IsFailure);
        Assert.True(assignResult.Value);

        // Act - Unassign distributor from point of sale
        var unassignCommand = new UnassignDistributorFromPointOfSaleCommand(
            posPhoneNumber,
            distributorPhoneNumber,
            category
        );
        var unassignResult = await Sender.Send(unassignCommand);

        // Assert
        Assert.False(unassignResult.IsFailure);
        Assert.True(unassignResult.Value);
    }

    [Fact]
    public async Task UnassignDistributor_WhenPointOfSaleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var unassignCommand = new UnassignDistributorFromPointOfSaleCommand(
            "+59891000999", // Non-existent point of sale
            "+59891000998", // Non-existent distributor
            Category.LACTEOS
        );

        // Act
        var result = await Sender.Send(unassignCommand);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UnassignDistributor_WhenDistributorNotFound_ShouldReturnFailure()
    {
        // Arrange - Create only point of sale
        var posPhoneNumber = "+59891000201";
        var createPosCommand = new CreatePointOfSaleCommand(
            "Punto Test NotFound",
            posPhoneNumber,
            "Montevideo",
            "Calle Test 201",
            "11000"
        );
        var posResult = await Sender.Send(createPosCommand);
        Assert.False(posResult.IsFailure);

        var unassignCommand = new UnassignDistributorFromPointOfSaleCommand(
            posPhoneNumber,
            "+59891000999", // Non-existent distributor
            Category.LACTEOS
        );

        // Act
        var result = await Sender.Send(unassignCommand);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UnassignDistributor_WhenAssignmentNotFound_ShouldReturnFailure()
    {
        // Arrange - Create both point of sale and distributor but don't assign them
        var posPhoneNumber = "+59891000301";
        var distributorPhoneNumber = "+59891000302";
        var category = Category.LACTEOS;

        // Create point of sale
        var createPosCommand = new CreatePointOfSaleCommand(
            "Punto Test NoAssignment",
            posPhoneNumber,
            "Montevideo",
            "Calle Test 301",
            "11000"
        );
        var posResult = await Sender.Send(createPosCommand);
        Assert.False(posResult.IsFailure);

        // Create distributor
        var createDistributorCommand = new CreateDistributorCommand(
            "Distribuidor Test NoAssignment",
            distributorPhoneNumber,
            "Dirección Test 301",
            new List<Category> { category }
        );
        var distributorResult = await Sender.Send(createDistributorCommand);
        Assert.False(distributorResult.IsFailure);

        // Act - Try to unassign without assignment
        var unassignCommand = new UnassignDistributorFromPointOfSaleCommand(
            posPhoneNumber,
            distributorPhoneNumber,
            category
        );
        var result = await Sender.Send(unassignCommand);

        // Assert
        Assert.True(result.IsFailure);
    }
}