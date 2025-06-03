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
using Conaprole.Orders.Application.Orders.GetOrders;
using Conaprole.Orders.Domain.Shared;
using Swashbuckle.AspNetCore.Annotations;

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
    [SwaggerOperation(Summary = "Get all distributors", Description = "Retrieves a list of all distributors with their summary information including supported categories and assigned points of sale count.")]
    [ProducesResponseType(typeof(List<DistributorSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistributors(CancellationToken cancellationToken)
    {
        var query = new GetDistributorsQuery();
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new distributor", Description = "Creates a new distributor with name, phone number, address and supported categories.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDistributor([FromBody] CreateDistributorRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDistributorCommand(
            request.Name,
            request.PhoneNumber,
            request.Address,
            request.Categories.Select(c => Enum.Parse<Category>(c, ignoreCase: true)).ToList());
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Created(string.Empty, result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{distPhoneNumber}/pos")]
    [SwaggerOperation(Summary = "Get assigned POS", Description = "Retrieves a list of points of sale assigned to the specified distributor.")]
    public async Task<IActionResult> GetAssignedPointsOfSale(string distPhoneNumber, CancellationToken cancellationToken)
    {
        var query = new GetAssignedPointsOfSaleQuery(distPhoneNumber);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{distPhoneNumber}/pos/{posPhoneNumber}")]
    [SwaggerOperation(Summary = "Get POS details", Description = "Retrieves details (phone, address, active status, creation date) of a specific point of sale assigned to the distributor.")]
    public async Task<IActionResult> GetPointOfSaleDetails(string distPhoneNumber, string posPhoneNumber, CancellationToken cancellationToken)
    {
        var query = new GetPointOfSaleDetailsQuery(distPhoneNumber, posPhoneNumber);
        var result = await _sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("{distPhoneNumber}/orders")]
    [SwaggerOperation(Summary = "Get orders for distributor", Description = "Returns a list of orders for a specific distributor, optionally filtered by point of sale.")]
    public async Task<IActionResult> GetOrders(string distPhoneNumber, [FromQuery] string? posPhoneNumber, CancellationToken cancellationToken)
    {
        var query = new GetOrdersQuery(null, null, null, distPhoneNumber, posPhoneNumber);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{distPhoneNumber}/categories")]
    [SwaggerOperation(Summary = "Get distributor categories", Description = "Returns the list of product categories that the distributor is authorized to deliver.")]
    public async Task<IActionResult> GetCategories(string distPhoneNumber, CancellationToken cancellationToken)
    {
        var query = new GetDistributorCategoriesQuery(distPhoneNumber);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result.Value);
    }

    [HttpPost("{distPhoneNumber}/categories")]
    [SwaggerOperation(Summary = "Add category to distributor", Description = "Assigns a new supported product category to the distributor.")]
    public async Task<IActionResult> AddCategory(string distPhoneNumber, [FromBody] AddDistributorCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new AddDistributorCategoryCommand(distPhoneNumber, Enum.Parse<Category>(request.Category, ignoreCase: true));
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{distPhoneNumber}/categories")]
    [SwaggerOperation(Summary = "Remove category from distributor", Description = "Revokes a product category previously assigned to the distributor.")]
    public async Task<IActionResult> RemoveCategory(string distPhoneNumber, [FromBody] RemoveDistributorCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new RemoveDistributorCategoryCommand(distPhoneNumber, Enum.Parse<Category>(request.Category, ignoreCase: true));
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}