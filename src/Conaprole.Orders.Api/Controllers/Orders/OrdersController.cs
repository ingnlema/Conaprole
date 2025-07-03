using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.Controllers.Orders.Examples;
using Conaprole.Orders.Api.Controllers.Orders.Dtos.Examples;
using Conaprole.Orders.Application.Orders.AddOrderLine;
using Conaprole.Orders.Application.Orders.BulkCreateOrders;
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
using Conaprole.Orders.Infrastructure.Authorization;

namespace Conaprole.Orders.Api.Controllers.Orders;

/// <summary>
/// Controller for managing dairy product orders
/// </summary>
[ApiController]
[Route("api/Orders")]
[ApiExplorerSettings(GroupName = "Orders")]
public class OrdersController : ControllerBase
{

    private readonly ISender _sender;
    
    /// <summary>
    /// Initializes a new instance of the OrdersController
    /// </summary>
    /// <param name="sender">MediatR sender for command and query handling</param>
    public OrdersController(ISender sender)
    {
        _sender = sender;
    }
    
    
    /// <summary>
    /// Retrieves a specific order by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The order details including all order lines</returns>
    /// <response code="200">Returns the requested order with all details</response>
    /// <response code="404">If the order with the specified ID is not found</response>
    [SwaggerOperation(Summary = "Obtener orden por ID", Description = "Devuelve una orden específica con todos los detalles y líneas.")]
    [HttpGet("{id}")]
    [HasPermission(Permissions.OrdersRead)]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
    
    /// <summary>
    /// Creates a new order with the specified details
    /// </summary>
    /// <param name="request">Order creation request containing all necessary details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The unique identifier of the created order</returns>
    /// <response code="201">Order created successfully. Returns the order ID.</response>
    /// <response code="400">Invalid request data or business rule violation</response>
    [SwaggerOperation(Summary = "Crear orden", Description = "Crea una nueva orden con dirección, distribuidor, punto de venta y líneas de orden.")]
    [HttpPost]
    [HasPermission(Permissions.OrdersWrite)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(CreateOrderRequest), typeof(CreateOrderRequestExample))]
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
    /// Crea múltiples órdenes en una sola transacción atómica.
    /// </summary>
    [SwaggerOperation(Summary = "Crear órdenes en lote", Description = "Crea múltiples órdenes de manera atómica. Si alguna orden falla, ninguna se crea.")]
    [HttpPost("bulk")]
    [HasPermission(Permissions.OrdersWrite)]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(BulkCreateOrdersRequest), typeof(BulkCreateOrdersRequestExample))]
    public async Task<IActionResult> CreateOrdersBulk(BulkCreateOrdersRequest request, CancellationToken cancellationToken)
    {
        if (request.Orders == null || !request.Orders.Any())
            return BadRequest(new Error("EmptyOrderList", "At least one order is required."));

        var orderCommands = request.Orders
            .Select(order => new CreateOrderCommand(
                order.PointOfSalePhoneNumber,
                order.DistributorPhoneNumber,
                order.City,
                order.Street,
                order.ZipCode,
                order.CurrencyCode,
                order.OrderLines
                    .Select(ol => new CreateOrderLineCommand(ol.ExternalProductId, ol.Quantity))
                    .ToList()))
            .ToList();

        var command = new BulkCreateOrdersCommand(orderCommands);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Created("", result.Value);
    }
    
    /// <summary>
    /// Actualiza el estado de una orden.
    /// </summary>
    [SwaggerOperation(Summary = "Actualizar estado de orden", Description = "Actualiza el estado de una orden (ej. Confirmada, Entregada).")]
    [HttpPut("{id}/status")]
    [HasPermission(Permissions.OrdersWrite)]
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
    /// Lista órdenes con filtros opcionales.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.OrdersRead)]
    [SwaggerOperation(
        Summary = "Obtener órdenes con filtros opcionales", 
        Description = "Filtrar por fecha, estado, distribuidor, punto de venta, o IDs específicos (separados por comas). " +
                     "Ejemplo: GET /api/orders?ids=guid1,guid2,guid3 para obtener órdenes específicas por sus IDs.")]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequest request, CancellationToken cancellationToken)

    {
        // Parse comma-separated IDs if provided
        List<Guid>? ids = null;
        if (!string.IsNullOrWhiteSpace(request.Ids))
        {
            var idStrings = request.Ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
            ids = new List<Guid>();
            
            foreach (var idString in idStrings)
            {
                if (Guid.TryParse(idString.Trim(), out var id))
                {
                    ids.Add(id);
                }
                else
                {
                    return BadRequest(new Error("InvalidIds", $"Invalid GUID format: {idString.Trim()}"));
                }
            }
        }

        var query = new GetOrdersQuery(
            request.From,
            request.To,
            request.Status,
            request.Distributor,
            request.PointOfSalePhoneNumber,
            ids);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Agrega una nueva línea a una orden existente.
    /// </summary>
    [SwaggerOperation(Summary = "Agregar línea de orden", Description = "Agrega una línea de producto a una orden existente.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [HttpPost("{orderId:guid}/lines")]
    [HasPermission(Permissions.OrdersWrite)]
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
    /// Remueve una línea de orden por su ID.
    /// </summary>
    [SwaggerOperation(Summary = "Eliminar línea de orden", Description = "Elimina una línea de orden específica por ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [HttpDelete("{orderId:guid}/lines/{orderLineId:guid}")]
    [HasPermission(Permissions.OrdersWrite)]
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
    /// Actualiza la cantidad de una línea de orden existente.
    /// </summary>
    [SwaggerOperation(Summary = "Actualizar cantidad de línea de orden", Description = "Actualiza la cantidad de una línea en una orden existente.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [HttpPut("{orderId:guid}/lines/{orderLineId:guid}")]
    [HasPermission(Permissions.OrdersWrite)]
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