using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.IntegrationTests.Products;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders;

/// <summary>
/// Encapsula datos y creación de una orden para tests de integración.
/// </summary>
public static class OrderData
{
    public const string DefaultCity = "Montevideo";
    public const string DefaultStreet = "Test Street 123";
    public const string DefaultZipCode = "11200";
    public const string DefaultCurrency = "UYU";

    /// <summary>
    /// Crea una orden completa con todos los datos necesarios.
    /// </summary>
    public static async Task<Guid> SeedAsync(ISender sender, string pointOfSalePhoneNumber)
    {
        // 1. Crear producto
        var productId = await ProductData.SeedAsync(sender);

        // 2. Crear distribuidor
        var distributorId = await DistributorData.SeedAsync(sender);

        // 3. Crear punto de venta
        var pointOfSaleId = await PointOfSaleData.SeedAsync(sender, pointOfSalePhoneNumber);

        // 4. Crear orden
        var orderLines = new List<CreateOrderLineCommand>
        {
            new(ProductData.ExternalProductId, 2)
        };

        var createOrderCommand = new CreateOrderCommand(
            pointOfSalePhoneNumber,
            DistributorData.DefaultPhoneNumber,
            DefaultCity,
            DefaultStreet,
            DefaultZipCode,
            DefaultCurrency,
            orderLines
        );

        var result = await sender.Send(createOrderCommand);
        if (result.IsFailure)
            throw new Exception($"Error seeding order: {result.Error.Code}");
        
        return result.Value;
    }

    /// <summary>
    /// Crea múltiples órdenes para un punto de venta específico.
    /// </summary>
    public static async Task<List<Guid>> SeedMultipleAsync(ISender sender, string pointOfSalePhoneNumber, int count = 3)
    {
        var orderIds = new List<Guid>();
        
        for (int i = 0; i < count; i++)
        {
            var orderId = await SeedAsync(sender, pointOfSalePhoneNumber);
            orderIds.Add(orderId);
        }
        
        return orderIds;
    }
}