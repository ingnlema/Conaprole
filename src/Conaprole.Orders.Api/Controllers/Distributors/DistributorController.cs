using Conaprole.Orders.Api.Controllers.Distributors.Dtos;
using Conaprole.Orders.Application.Distributors.AddCategory;
using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Application.Distributors.GetAssignedPointsOfSale;
using Conaprole.Orders.Application.Distributors.GetCategories;
using Conaprole.Orders.Application.Distributors.GetDistributors;
using Conaprole.Orders.Application.Distributors.GetPointOfSaleDetails;
using Conaprole.Orders.Application.Distributors.RemoveCategory;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Conaprole.Orders.Application.Distributors.GetOrdersForDistributor;
using Conaprole.Orders.Domain.Shared;
using Swashbuckle.AspNetCore.Annotations;
using Conaprole.Orders.Infrastructure.Authorization;

namespace Conaprole.Orders.Api.Controllers.Distributors;


[ApiController]
[Route("api/distributors")]
public class DistributorController : ControllerBase
{
    private readonly ISender _sender;

    public DistributorController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [HasPermission(Permissions.DistributorsRead)]
    [SwaggerOperation(Summary = "Obtener todos los distribuidores", Description = "Recupera una lista de todos los distribuidores con su información resumida incluyendo categorías soportadas y cantidad de puntos de venta asignados.")]
    [ProducesResponseType(typeof(List<DistributorSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistributors(CancellationToken cancellationToken)
    {
        var query = new GetDistributorsQuery();
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission(Permissions.DistributorsWrite)]
    [SwaggerOperation(Summary = "Crear un nuevo distribuidor", Description = "Crea un nuevo distribuidor con nombre, número de teléfono, dirección y categorías soportadas.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDistributor([FromBody] CreateDistributorRequest request, CancellationToken cancellationToken)
    {
        var categoriesResult = CategoryHelper.TryParseCategories(request.Categories, this);
        if (categoriesResult?.Result != null)
        {
            return categoriesResult.Result; // Return the error response
        }

        var command = new CreateDistributorCommand(
            request.Name,
            request.PhoneNumber,
            request.Address,
            categoriesResult!.Value!);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Created(string.Empty, result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{distPhoneNumber}/pos")]
    [HasPermission(Permissions.DistributorsRead)]
    [SwaggerOperation(Summary = "Obtener puntos de venta asignados", Description = "Recupera una lista de puntos de venta asignados al distribuidor especificado.")]
    public async Task<IActionResult> GetAssignedPointsOfSale(string distPhoneNumber, CancellationToken cancellationToken)
    {
        var query = new GetAssignedPointsOfSaleQuery(distPhoneNumber);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{distPhoneNumber}/pos/{posPhoneNumber}")]
    [HasPermission(Permissions.DistributorsRead)]
    [SwaggerOperation(Summary = "Obtener detalles del punto de venta", Description = "Recupera los detalles (teléfono, dirección, estado activo, fecha de creación) de un punto de venta específico asignado al distribuidor.")]
    public async Task<IActionResult> GetPointOfSaleDetails(string distPhoneNumber, string posPhoneNumber, CancellationToken cancellationToken)
    {
        var query = new GetPointOfSaleDetailsQuery(distPhoneNumber, posPhoneNumber);
        var result = await _sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("{distPhoneNumber}/orders")]
    [HasPermission(Permissions.OrdersRead)]
    [SwaggerOperation(Summary = "Obtener órdenes para distribuidor", Description = "Devuelve una lista de órdenes para un distribuidor específico, opcionalmente filtradas por punto de venta.")]
    public async Task<IActionResult> GetOrders(string distPhoneNumber, [FromQuery] string? posPhoneNumber, CancellationToken cancellationToken)
    {
        var query = new GetOrdersForDistributorQuery(distPhoneNumber, posPhoneNumber);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{distPhoneNumber}/categories")]
    [HasPermission(Permissions.DistributorsRead)]
    [SwaggerOperation(Summary = "Obtener categorías del distribuidor", Description = "Devuelve la lista de categorías de productos que el distribuidor está autorizado a entregar.")]
    public async Task<IActionResult> GetCategories(string distPhoneNumber, CancellationToken cancellationToken)
    {
        var query = new GetDistributorCategoriesQuery(distPhoneNumber);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost("{distPhoneNumber}/categories")]
    [HasPermission(Permissions.DistributorsWrite)]
    [SwaggerOperation(Summary = "Agregar categoría al distribuidor", Description = "Asigna una nueva categoría de producto soportada al distribuidor.")]
    public async Task<IActionResult> AddCategory(string distPhoneNumber, [FromBody] AddDistributorCategoryRequest request, CancellationToken cancellationToken)
    {
        var categoryResult = CategoryHelper.TryParseCategory(request.Category, this);
        if (categoryResult?.Result != null)
        {
            return categoryResult.Result; // Return the error response
        }

        var command = new AddDistributorCategoryCommand(distPhoneNumber, categoryResult!.Value);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{distPhoneNumber}/categories/{category}")]
    [HasPermission(Permissions.DistributorsWrite)]
    [SwaggerOperation(Summary = "Remover categoría del distribuidor", Description = "Revoca una categoría de producto previamente asignada al distribuidor.")]
    public async Task<IActionResult> RemoveCategory(string distPhoneNumber, string category, CancellationToken cancellationToken)
    {
        var categoryResult = CategoryHelper.TryParseCategory(category, this);
        if (categoryResult?.Result != null)
        {
            return categoryResult.Result; // Return the error response
        }

        var command = new RemoveDistributorCategoryCommand(distPhoneNumber, categoryResult!.Value);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{distPhoneNumber}/categories")]
    [HasPermission(Permissions.DistributorsWrite)]
    [SwaggerOperation(Summary = "Remover categoría del distribuidor (legacy)", Description = "Revoca una categoría de producto previamente asignada al distribuidor. Versión legacy con body.")]
    [Obsolete("Use DELETE /api/distributors/{distPhoneNumber}/categories/{category} instead")]
    public async Task<IActionResult> RemoveCategoryLegacy(string distPhoneNumber, [FromBody] RemoveDistributorCategoryRequest request, CancellationToken cancellationToken)
    {
        var categoryResult = CategoryHelper.TryParseCategory(request.Category, this);
        if (categoryResult?.Result != null)
        {
            return categoryResult.Result; // Return the error response
        }

        var command = new RemoveDistributorCategoryCommand(distPhoneNumber, categoryResult!.Value);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}