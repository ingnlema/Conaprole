using Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;
using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.EnablePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetPointsOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetDistributorsByPointOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetPointOfSaleByPhoneNumber;
using Conaprole.Orders.Application.PointsOfSale.UnassignDistributor;
using Conaprole.Orders.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Conaprole.Orders.Infrastructure.Authorization;

namespace Conaprole.Orders.Api.Controllers.PointsOfSale;

[ApiController]
[Route("api/pos")]
public class PointOfSaleController : ControllerBase
{
    private readonly ISender _sender;

    public PointOfSaleController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [HasPermission(Permissions.PointsOfSaleRead)]
    [SwaggerOperation(
        Summary = "Listar puntos de venta por estado", 
        Description = "Recupera puntos de venta filtrados por estado. Soporta filtros por activos, inactivos, o todos los puntos de venta. Por defecto muestra activos si no se especifica estado.")]
    public async Task<IActionResult> GetPointsOfSale(
        [FromQuery, SwaggerParameter(
            "Filtrar por estado del punto de venta. Valores válidos: 'active' (por defecto), 'inactive', 'all'. Valores inválidos se convierten por defecto a 'active'.")] 
        string? status = "active", 
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetPointsOfSaleQuery(status), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("by-phone/{phoneNumber}")]
    [HasPermission(Permissions.PointsOfSaleRead)]
    [SwaggerOperation(Summary = "Obtener punto de venta por teléfono", Description = "Recupera un punto de venta por su número de teléfono. Devuelve los datos del punto de venta incluyendo su estado activo.")]
    public async Task<IActionResult> GetPointOfSaleByPhoneNumber(string phoneNumber, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPointOfSaleByPhoneNumberQuery(phoneNumber), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("{posPhoneNumber}/distributors")]
    [HasPermission(Permissions.PointsOfSaleRead)]
    [SwaggerOperation(Summary = "Obtener distribuidores por punto de venta", Description = "Devuelve la lista de distribuidores asignados a un punto de venta específico, incluyendo categorías de productos.")]
    public async Task<IActionResult> GetDistributorsByPOS(string posPhoneNumber, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDistributorsByPointOfSaleQuery(posPhoneNumber), cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Crear punto de venta", Description = "Registra un nuevo punto de venta con su información de contacto y dirección.")]
    public async Task<IActionResult> Create([FromBody] CreatePointOfSaleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePointOfSaleCommand(request.Name, request.PhoneNumber, request.City, request.Street, request.ZipCode);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetPointsOfSale), new { }, result.Value);
    }

    [HttpPatch("{posPhoneNumber}")]
    [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Desactivar punto de venta", Description = "Desactiva un punto de venta para prevenir que reciba nuevas órdenes.")]
    public async Task<IActionResult> Disable(string posPhoneNumber, CancellationToken cancellationToken)
    {
        var command = new DisablePointOfSaleCommand(posPhoneNumber);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPatch("{posPhoneNumber}/enable")]
    [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Activar punto de venta", Description = "Activa un punto de venta para permitir que reciba nuevas órdenes.")]
    public async Task<IActionResult> Enable(string posPhoneNumber, CancellationToken cancellationToken)
    {
        var command = new EnablePointOfSaleCommand(posPhoneNumber);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{posPhoneNumber}/distributors")]
    [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Asignar distribuidor al punto de venta", Description = "Asigna un distribuidor a un punto de venta para una categoría de producto específica.")]
    public async Task<IActionResult> AssignDistributor(string posPhoneNumber, [FromBody] AssignDistributorToPointOfSaleRequest request, CancellationToken cancellationToken)
    {
        var categoryResult = CategoryHelper.TryParseCategory(request.Category, this);
        if (categoryResult?.Result != null)
        {
            return categoryResult.Result; // Return the error response
        }

        var command = new AssignDistributorToPointOfSaleCommand(posPhoneNumber, request.DistributorPhoneNumber, categoryResult!.Value);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{posPhoneNumber}/distributors/{distributorPhoneNumber}/categories/{category}")]
    [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Desasignar distribuidor del punto de venta", Description = "Remueve la asociación de un distribuidor con un punto de venta para una categoría de producto dada.")]
    public async Task<IActionResult> UnassignDistributor(string posPhoneNumber, string distributorPhoneNumber, Category category, CancellationToken cancellationToken)
    {
        var command = new UnassignDistributorFromPointOfSaleCommand(posPhoneNumber, distributorPhoneNumber, category);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{posPhoneNumber}/distributors")]
    [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Desasignar distribuidor del punto de venta (legacy)", Description = "Remueve la asociación de un distribuidor con un punto de venta usando request body. Mantenido para compatibilidad con clientes externos.")]
    public async Task<IActionResult> UnassignDistributorLegacy(string posPhoneNumber, [FromBody] UnassignDistributorFromPointOfSaleRequest request, CancellationToken cancellationToken)
    {
        var categoryResult = CategoryHelper.TryParseCategory(request.Category, this);
        if (categoryResult?.Result != null)
        {
            return categoryResult.Result; // Return the error response
        }

        var command = new UnassignDistributorFromPointOfSaleCommand(posPhoneNumber, request.DistributorPhoneNumber, categoryResult!.Value);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
    

}