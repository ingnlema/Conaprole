using Conaprole.Orders.Application.PointsOfSale.GetPointOfSaleByPhoneNumber;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.PointsOfSale;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class GetPointOfSaleByPhoneNumberTest : BaseIntegrationTest
    {
        public GetPointOfSaleByPhoneNumberTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetPointOfSaleByPhoneNumberQuery_Should_Return_PointOfSale_When_Exists()
        {
            // Arrange
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // Act
            var result = await Sender.Send(new GetPointOfSaleByPhoneNumberQuery(PointOfSaleData.PhoneNumber));

            // Assert
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            
            var pointOfSale = result.Value;
            Assert.Equal(pointOfSaleId, pointOfSale.Id);
            Assert.Equal(PointOfSaleData.Name, pointOfSale.Name);
            Assert.Equal(PointOfSaleData.PhoneNumber, pointOfSale.PhoneNumber);
            Assert.True(pointOfSale.IsActive);
            Assert.True(pointOfSale.CreatedAt > DateTime.MinValue);
            
            // Verify address contains expected data
            Assert.Contains(PointOfSaleData.Street, pointOfSale.Address);
            Assert.Contains(PointOfSaleData.City, pointOfSale.Address);
            Assert.Contains(PointOfSaleData.ZipCode, pointOfSale.Address);
        }

        [Fact]
        public async Task GetPointOfSaleByPhoneNumberQuery_Should_Return_NotFound_When_PhoneNumber_DoesNotExist()
        {
            // Arrange
            var nonExistentPhoneNumber = "+59899999999";

            // Act
            var result = await Sender.Send(new GetPointOfSaleByPhoneNumberQuery(nonExistentPhoneNumber));

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PointOfSaleErrors.NotFound, result.Error);
        }

        [Fact]
        public async Task GetPointOfSaleByPhoneNumberQuery_Should_Return_PointOfSale_Even_When_Inactive()
        {
            // Arrange - Create and then disable a POS
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);
            
            // Disable the POS
            var disableCommand = new Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale.DisablePointOfSaleCommand(PointOfSaleData.PhoneNumber);
            await Sender.Send(disableCommand);

            // Act
            var result = await Sender.Send(new GetPointOfSaleByPhoneNumberQuery(PointOfSaleData.PhoneNumber));

            // Assert
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            
            var pointOfSale = result.Value;
            Assert.Equal(pointOfSaleId, pointOfSale.Id);
            Assert.Equal(PointOfSaleData.Name, pointOfSale.Name);
            Assert.Equal(PointOfSaleData.PhoneNumber, pointOfSale.PhoneNumber);
            Assert.False(pointOfSale.IsActive); // Should be inactive
        }
    }
}