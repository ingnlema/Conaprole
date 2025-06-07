using Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class DisablePointOfSaleTest : BaseIntegrationTest
    {
        public DisablePointOfSaleTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task DisablePointOfSaleCommand_WithActivePointOfSale_ShouldReturnSuccessAndDeactivate()
        {
            // 1) Sembrar el punto de venta y obtener su ID
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // 2) Verificar que inicialmente está activo usando GetActivePointsOfSale
            var activePointsResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            Assert.False(activePointsResult.IsFailure);
            
            var activePoints = activePointsResult.Value;
            var initialPoint = activePoints.First(p => p.Id == pointOfSaleId);
            Assert.NotNull(initialPoint);
            Assert.True(initialPoint.IsActive);

            // 3) Ejecutar el comando de desactivación
            var disableCommand = new DisablePointOfSaleCommand(PointOfSaleData.PhoneNumber);
            var disableResult = await Sender.Send(disableCommand);
            
            // 4) Verificar que el resultado sea exitoso
            Assert.False(disableResult.IsFailure);
            Assert.True(disableResult.Value);

            // 5) Verificar que el punto de venta ya no aparece en la lista de activos
            var activePointsAfterResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            Assert.False(activePointsAfterResult.IsFailure);
            
            var activePointsAfter = activePointsAfterResult.Value;
            var disabledPoint = activePointsAfter.FirstOrDefault(p => p.Id == pointOfSaleId);
            Assert.Null(disabledPoint); // No debe aparecer en la lista de activos
        }

        [Fact]
        public async Task DisablePointOfSaleCommand_WithNonExistentPointOfSale_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentPhoneNumber = "+59899999999";
            var disableCommand = new DisablePointOfSaleCommand(nonExistentPhoneNumber);

            // Act
            var result = await Sender.Send(disableCommand);

            // Assert
            Assert.True(result.IsFailure);
        }
    }
}