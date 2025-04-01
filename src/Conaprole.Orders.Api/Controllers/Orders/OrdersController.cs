using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.Orders.GetOrders;
using Conaprole.Orders.Domain.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Conaprole.Orders.Api.Controllers.Orders;

[ApiController]
[Route("api/Orders")]
public class OrdersController : ControllerBase
{

    private readonly ISender _sender;
    
    public OrdersController(ISender sender)
    {
        _sender = sender;
    }
    
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.PointOfSaleId,
            request.Distributor,
            request.City,
            request.Street,
            request.ZipCode,
            request.CurrencyCode,
            request.OrderLines
                .Select(ol => new CreateOrderLineCommand(ol.ProductId, ol.Quantity))
                .ToList());

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetOrder), new { id = result.Value }, result.Value);
    }
    
    [HttpGet]
    [HttpGet]
    [SwaggerOperation(Summary = "Gets orders with optional filters", Description = "Filter by date, status, distributor or point of sale.")]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequest request, CancellationToken cancellationToken)

    {
        var query = new GetOrdersQuery(
            request.From,
            request.To,
            request.Status,
            request.Distributor,
            request.PointOfSaleId);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }



}