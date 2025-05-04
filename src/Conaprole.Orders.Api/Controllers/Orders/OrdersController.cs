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
            request.PointOfSalePhoneNumber,
            request.Distributor,
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

    
    [HttpGet]
    [SwaggerOperation(Summary = "Gets orders with optional filters", Description = "Filter by date, status, distributor or point of sale.")]
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
    /// POST /api/Orders/{orderId}/lines
    /// Ahora sólo necesita { ExternalProductId, Quantity }.
    /// </summary>
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
    /// DELETE /api/Orders/{orderId}/lines/{orderLineId}
    /// Permite eliminar una línea existente por su Id.
    /// </summary>
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
    /// PUT /api/Orders/{orderId}/lines/{orderLineId}
    /// Sólo necesita la nueva cantidad en el body.
    /// </summary>
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