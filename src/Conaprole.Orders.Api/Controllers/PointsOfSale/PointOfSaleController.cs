using Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;
using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetDistributorsByPointOfSale;
using Conaprole.Orders.Application.PointsOfSale.UnassignDistributor;
using Conaprole.Orders.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
// using Conaprole.Orders.Infrastructure.Authorization;

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
    // [HasPermission(Permissions.PointsOfSaleRead)]
    [SwaggerOperation(Summary = "List all active POS", Description = "Retrieves all active points of sale.")]
    public async Task<IActionResult> GetActivePointsOfSale(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetActivePointsOfSaleQuery(), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{posPhoneNumber}/distributors")]
    // [HasPermission(Permissions.PointsOfSaleRead)]
    [SwaggerOperation(Summary = "Get distributors by POS", Description = "Returns the list of distributors assigned to a specific point of sale, including product categories.")]
    public async Task<IActionResult> GetDistributorsByPOS(string posPhoneNumber, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDistributorsByPointOfSaleQuery(posPhoneNumber), cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost]
    // [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Create POS", Description = "Registers a new point of sale with its contact and address information.")]
    public async Task<IActionResult> Create([FromBody] CreatePointOfSaleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePointOfSaleCommand(request.Name, request.PhoneNumber, request.City, request.Street, request.ZipCode);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetActivePointsOfSale), new { }, result.Value);
    }

    [HttpPatch("{posPhoneNumber}")]
    // [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Disable POS", Description = "Disables a point of sale to prevent it from receiving new orders.")]
    public async Task<IActionResult> Disable(string posPhoneNumber, CancellationToken cancellationToken)
    {
        var command = new DisablePointOfSaleCommand(posPhoneNumber);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{posPhoneNumber}/distributors")]
    // [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Assign distributor to POS", Description = "Assigns a distributor to a point of sale for a specific product category.")]
    public async Task<IActionResult> AssignDistributor(string posPhoneNumber, [FromBody] AssignDistributorToPointOfSaleRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignDistributorToPointOfSaleCommand(posPhoneNumber, request.DistributorPhoneNumber, Enum.Parse<Category>(request.Category, ignoreCase: true));
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{posPhoneNumber}/distributors/{distributorPhoneNumber}/categories/{category}")]
    // [HasPermission(Permissions.PointsOfSaleWrite)]
    [SwaggerOperation(Summary = "Unassign distributor from POS", Description = "Removes the association of a distributor with a point of sale for a given product category.")]
    public async Task<IActionResult> UnassignDistributor(string posPhoneNumber, string distributorPhoneNumber, Category category, CancellationToken cancellationToken)
    {
        var command = new UnassignDistributorFromPointOfSaleCommand(posPhoneNumber, distributorPhoneNumber, category);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }


}