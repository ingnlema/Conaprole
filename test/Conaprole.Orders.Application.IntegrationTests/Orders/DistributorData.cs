using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Domain.Shared;
using MediatR;

namespace Conaprole.Orders.Application.IntegrationTests.Orders;

/// <summary>
/// Encapsula datos y creación de un distribuidor para tests de integración.
/// </summary>
public static class DistributorData
{
    public const string DefaultPhoneNumber = "+59899887766";
    public const string DefaultName = "Distribuidor Test";
    public const string DefaultAddress = "Distributor Test Address";
    public static readonly List<Category> DefaultCategories = new() { Category.LACTEOS };

    /// <summary>
    /// Comando preconfigurado para crear el distribuidor.
    /// </summary>
    public static CreateDistributorCommand CreateCommand =>
        new(
            DefaultName,
            DefaultPhoneNumber,
            DefaultAddress,
            DefaultCategories
        );

    /// <summary>
    /// Crea el distribuidor vía MediatR y devuelve su ID.
    /// </summary>
    public static async Task<Guid> SeedAsync(ISender sender)
    {
        var result = await sender.Send(CreateCommand);
        if (result.IsFailure)
            throw new Exception($"Error seeding distributor: {result.Error.Code}");
        return result.Value;
    }

    /// <summary>
    /// Crea el distribuidor con un número de teléfono específico.
    /// </summary>
    public static async Task<Guid> SeedAsync(ISender sender, string phoneNumber)
    {
        var command = new CreateDistributorCommand(
            DefaultName,
            phoneNumber,
            DefaultAddress,
            DefaultCategories
        );
        
        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new Exception($"Error seeding distributor: {result.Error.Code}");
        return result.Value;
    }
}