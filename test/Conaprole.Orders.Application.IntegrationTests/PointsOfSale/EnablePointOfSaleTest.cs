using Conaprole.Orders.Application.PointsOfSale.EnablePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.PointsOfSale;
using Dapper;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class EnablePointOfSaleTest : BaseIntegrationTest
    {
        public EnablePointOfSaleTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task EnablePointOfSaleCommand_WithInactivePointOfSale_ShouldReturnSuccessAndActivate()
        {
            // 1) Sembrar el punto de venta y desactivarlo usando el comando apropiado
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);
            
            // Use the proper DisablePointOfSaleCommand instead of direct SQL manipulation
            var disableCommand = new DisablePointOfSaleCommand(PointOfSaleData.PhoneNumber);
            var disableResult = await Sender.Send(disableCommand);
            Assert.False(disableResult.IsFailure, $"Failed to disable point of sale: {disableResult.Error?.Code}");

            // 2) Verificar que inicialmente está inactivo usando GetActivePointsOfSale
            var activePointsResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            Assert.False(activePointsResult.IsFailure);
            
            var activePoints = activePointsResult.Value;
            var initialPoint = activePoints.FirstOrDefault(p => p.Id == pointOfSaleId);
            Assert.Null(initialPoint); // No debe aparecer en la lista de activos

            // 3) Ejecutar el comando de activación
            var enableCommand = new EnablePointOfSaleCommand(PointOfSaleData.PhoneNumber);
            var enableResult = await Sender.Send(enableCommand);
            
            // 4) Verificar que el resultado sea exitoso
            Assert.False(enableResult.IsFailure);
            Assert.True(enableResult.Value);

            // 5) Verificar que el punto de venta ahora aparece en la lista de activos
            var activePointsAfterResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            Assert.False(activePointsAfterResult.IsFailure);
            
            var activePointsAfter = activePointsAfterResult.Value;
            var enabledPoint = activePointsAfter.FirstOrDefault(p => p.Id == pointOfSaleId);
            Assert.NotNull(enabledPoint);
            Assert.True(enabledPoint.IsActive);
        }

        [Fact]
        public async Task EnablePointOfSaleCommand_WithNonExistentPointOfSale_ShouldReturnFailure()
        {
            // Arrange
            var nonExistentPhoneNumber = "+59899999997";
            var enableCommand = new EnablePointOfSaleCommand(nonExistentPhoneNumber);

            // Act
            var result = await Sender.Send(enableCommand);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PointOfSaleErrors.NotFound.Code, result.Error.Code);
        }

        [Fact]
        public async Task EnablePointOfSaleCommand_WithAlreadyActivePointOfSale_ShouldReturnFailure()
        {
            // Arrange - Crear un punto de venta activo
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // Verificar que está activo
            var activePointsResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            Assert.False(activePointsResult.IsFailure);
            var activePoint = activePointsResult.Value.First(p => p.Id == pointOfSaleId);
            Assert.True(activePoint.IsActive);

            // Act - Intentar activar un punto de venta ya activo
            var enableCommand = new EnablePointOfSaleCommand(PointOfSaleData.PhoneNumber);
            var result = await Sender.Send(enableCommand);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PointOfSaleErrors.AlreadyEnabled.Code, result.Error.Code);
        }

    }
}