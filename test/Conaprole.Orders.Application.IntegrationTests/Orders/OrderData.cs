using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders;

/// <summary>
/// Encapsula datos y creación de una orden para tests de integración.
/// </summary>
public static class OrderData
{
    public const string City = "Montevideo";
    public const string Street = "Calle Order 789";
    public const string ZipCode = "11300";
    public const string CurrencyCode = "UYU";

    /// <summary>
    /// Comando preconfigurado para crear la orden.
    /// </summary>
    public static CreateOrderCommand CreateCommand =>
        new(
            PointOfSaleData.PhoneNumber,
            DistributorData.PhoneNumber,
            City,
            Street,
            ZipCode,
            CurrencyCode,
            new List<CreateOrderLineCommand>
            {
                new(ProductData.ExternalProductId, 2)
            }
        );

    /// <summary>
    /// Crea una orden completa vía MediatR y devuelve su ID.
    /// Semilla las dependencias necesarias: Product, Distributor, PointOfSale.
    /// </summary>
    public static async Task<Guid> SeedAsync(ISender sender)
    {
        // Sembrar dependencias primero (idempotente)
        await SeedDependenciesAsync(sender);

        // Crear la orden
        var result = await sender.Send(CreateCommand);
        if (result.IsFailure)
            throw new Exception($"Error seeding order: {result.Error.Code}");
        return result.Value;
    }

    /// <summary>
    /// Semilla las dependencias de forma idempotente.
    /// </summary>
    private static async Task SeedDependenciesAsync(ISender sender)
    {
        try { await ProductData.SeedAsync(sender); } catch { /* Ya existe */ }
        try { await DistributorData.SeedAsync(sender); } catch { /* Ya existe */ }
        try { await PointOfSaleData.SeedAsync(sender); } catch { /* Ya existe */ }
    }
}