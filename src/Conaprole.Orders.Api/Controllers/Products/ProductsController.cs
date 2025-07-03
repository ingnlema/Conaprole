using Conaprole.Orders.Application.Products.CreateProduct;
using Conaprole.Orders.Application.Products.GetProduct;
using Conaprole.Orders.Application.Products.GetProducts;
using Conaprole.Orders.Domain.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ProductResponse = Conaprole.Orders.Application.Products.GetProduct.ProductResponse;
using Conaprole.Orders.Infrastructure.Authorization;

namespace Conaprole.Orders.Api.Controllers.Products;

/// <summary>
/// Controller for managing dairy products in the catalog
/// </summary>
[ApiController]
[Route("api/Products")]
[ApiExplorerSettings(GroupName = "Products")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;
    
    /// <summary>
    /// Initializes a new instance of the ProductsController
    /// </summary>
    /// <param name="sender">MediatR sender for command and query handling</param>
    public ProductsController(ISender sender)
    {
        _sender = sender;
    }
    
    /// <summary>
    /// Obtiene un producto por su identificador único.
    /// </summary>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ProductsRead)]
    [SwaggerOperation(Summary = "Obtener producto por ID", Description = "Devuelve un producto específico dado su ID")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        
        return Ok(result.Value);
    }
    
    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    /// <remarks>
    /// Requiere un ID de producto externo, nombre, precio unitario, moneda, descripción y un valor de categoría enum.
    /// </remarks>
    [HttpPost]
    [HasPermission(Permissions.ProductsWrite)]
    [SwaggerOperation(Summary = "Crear un nuevo producto", Description = "Crea un producto con una categoría específica")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.ExternalProductId,
            request.Name,
            request.UnitPrice,
            request.CurrencyCode,
            request.Description,
            request.Category);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetProduct), new { id = result.Value }, result.Value);
    }
    
    /// <summary>
    /// Lista todos los productos.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.ProductsRead)]
    [SwaggerOperation(Summary = "Obtener todos los productos", Description = "Devuelve una lista de todos los productos registrados")]
    [ProducesResponseType(typeof(List<ProductsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result.Value);
    }

}