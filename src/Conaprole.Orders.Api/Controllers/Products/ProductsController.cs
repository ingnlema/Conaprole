using Conaprole.Orders.Application.Products.CreateProduct;
using Conaprole.Orders.Application.Products.GetProduct;
using Conaprole.Orders.Application.Products.GetProducts;
using Conaprole.Orders.Domain.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ProductResponse = Conaprole.Orders.Application.Products.GetProduct.ProductResponse;

namespace Conaprole.Orders.Api.Controllers.Products;


[ApiController]
[Route("api/Products")]
public class ProductsController : ControllerBase
{

    private readonly ISender _sender;
    
    public ProductsController(ISender sender)
    {
        _sender = sender;
    }
    
    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get product by ID", Description = "Returns a single product given its ID")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        
        return Ok(result.Value);
    }
    
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <remarks>
    /// Requires an external product ID, name, unit price, currency, description, and a category enum value.
    /// </remarks>
    [HttpPost]
    [SwaggerOperation(Summary = "Creates a new product", Description = "Creates a product with a specific category")]
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
    /// Lists all products.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all products", Description = "Returns a list of all registered products")]
    [ProducesResponseType(typeof(List<ProductsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result.Value);
    }

}