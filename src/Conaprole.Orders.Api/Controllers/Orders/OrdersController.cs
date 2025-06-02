using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.Controllers.Orders.Examples;
using Conaprole.Orders.Application.Orders.AddOrderLine;
using Conaprole.Orders.Application.Orders.CreateOrder;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.Orders.GetOrders;
using Conaprole.Orders.Application.Orders.RemoveOrderLine;
using Conaprole.Orders.Application.Orders.UpdateOrderLineQuantity;
using Conaprole.Orders.Application.Orders.UpdateOrderStatus;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Conaprole.Orders.Api.Controllers.Orders;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{

    private readonly ISender _sender;
    
    public OrdersController(ISender sender)
    {
        _sender = sender;
    }
    
    
    /// <summary>
    /// Gets the details of a specific order by its ID.
    /// </summary>
    [SwaggerOperation(Summary = "Get order by ID", Description = "Returns a specific order with all details and lines.")]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
    
    /// <summary>
    /// Creates a new order.
    /// </summary>
    [SwaggerOperation(Summary = "Create order", Description = "Creates a new order with address, distributor, POS and order lines.")]
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.PointOfSalePhoneNumber,
            request.DistributorPhoneNumber,
            request.City,
            request.Street,
            request.ZipCode,
            request.CurrencyCode,
            request.OrderLines
                .Select(ol => new CreateOrderLineCommand(ol.ExternalProductId, ol.Quantity))
                .ToList());

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetOrder), new { id = result.Value }, result.Value);
    }
    
    /// <summary>
    /// Updates the status of an order.
    /// </summary>
    [SwaggerOperation(Summary = "Update order status", Description = "Updates the status of an order (e.g. Confirmed, Delivered).")]
    [HttpPut("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(UpdateOrderStatusRequest), typeof(UpdateOrderStatusRequestExample))]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (!request.NewStatus.HasValue)
            return BadRequest(new Error("InvalidInput", "El campo NewStatus es obligatorio."));
        
        if (!Enum.IsDefined(typeof(Status), request.NewStatus.Value))
            return BadRequest(new Error("InvalidStatus", "El valor de NewStatus no es válido."));

        var newStatus = (Status)request.NewStatus.Value;
        var command = new UpdateOrderStatusCommand(id, newStatus);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    
    /// <summary>
    /// Lists orders with optional filters.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Gets orders with optional filters", Description = "Filter by date, status, distributor or point of sale.")]
    [ProducesResponseType(typeof(List<OrderSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequest request, CancellationToken cancellationToken)

    {
        var query = new GetOrdersQuery(
            request.From,
            request.To,
            request.Status,
            request.Distributor,
            request.PointOfSalePhoneNumber);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a new line to an existing order.
    /// </summary>
    [SwaggerOperation(Summary = "Add order line", Description = "Adds a product line to an existing order.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [HttpPost("{orderId:guid}/lines")]
    public async Task<IActionResult> AddLine(
        Guid orderId,
        [FromBody] AddOrderLineRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var command = new AddOrderLineToOrderCommand(
            orderId,
            new ExternalProductId(request.ExternalProductId),
            request.Quantity
        );

        var result = await _sender.Send(command);
        if (result.IsFailure)
            return NotFound(result.Error);

        return NoContent();
    }

    /// <summary>
    /// Removes an order line by its ID.
    /// </summary>
    [SwaggerOperation(Summary = "Delete order line", Description = "Deletes a specific order line by ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [HttpDelete("{orderId:guid}/lines/{orderLineId:guid}")]
    public async Task<IActionResult> DeleteLine(
        Guid orderId,
        Guid orderLineId)
    {
        var result = await _sender.Send(new RemoveOrderLineFromOrderCommand(
            orderId,
            orderLineId
        ));

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "OrderLine.LastLine" => BadRequest(result.Error),
                "OrderLine.NotFound" => NotFound(result.Error),
                _                    => BadRequest(result.Error)
            };
        }

        return NoContent();
    }


    /// <summary>
    /// Updates the quantity of an existing order line.
    /// </summary>
    [SwaggerOperation(Summary = "Update order line quantity", Description = "Updates the quantity of a line in an existing order.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [HttpPut("{orderId:guid}/lines/{orderLineId:guid}")]
    public async Task<IActionResult> UpdateLineQuantity(
        Guid orderId,
        Guid orderLineId,
        [FromBody] UpdateOrderLineQuantityRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _sender.Send(new UpdateOrderLineQuantityCommand(
            orderId,
            orderLineId,
            request.NewQuantity
        ));

        if (result.IsFailure)
            return NotFound(result.Error);

        return NoContent();
    }



}