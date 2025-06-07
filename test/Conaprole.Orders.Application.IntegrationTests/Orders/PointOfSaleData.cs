using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Domain.Shared;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders;

/// <summary>
/// Encapsula datos y creación de un punto de venta para tests de integración.
/// </summary>
public static class PointOfSaleData
{
    public const string DefaultPhoneNumber = "+59891234567";
    public const string DefaultName = "POS Test";
    public const string DefaultCity = "Montevideo";
    public const string DefaultStreet = "Test Street 123";
    public const string DefaultZipCode = "11200";

    /// <summary>
    /// Comando preconfigurado para crear el punto de venta.
    /// </summary>
    public static CreatePointOfSaleCommand CreateCommand =>
        new(
            DefaultName,
            DefaultPhoneNumber,
            DefaultCity,
            DefaultStreet,
            DefaultZipCode
        );

    /// <summary>
    /// Crea el punto de venta vía MediatR y devuelve su ID.
    /// </summary>
    public static async Task<Guid> SeedAsync(ISender sender)
    {
        var result = await sender.Send(CreateCommand);
        if (result.IsFailure)
            throw new Exception($"Error seeding point of sale: {result.Error.Code}");
        return result.Value;
    }

    /// <summary>
    /// Crea el punto de venta con un número de teléfono específico.
    /// </summary>
    public static async Task<Guid> SeedAsync(ISender sender, string phoneNumber)
    {
        var command = new CreatePointOfSaleCommand(
            DefaultName,
            phoneNumber,
            DefaultCity,
            DefaultStreet,
            DefaultZipCode
        );
        
        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new Exception($"Error seeding point of sale: {result.Error.Code}");
        return result.Value;
    }
}