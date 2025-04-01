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
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        
        return Ok(result.Value);
    }
    
    [HttpPost]
    [SwaggerOperation(Summary = "Creates a new product", Description = "Creates a product with optional categories")]
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
            request.Categories);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetProduct), new { id = result.Value }, result.Value);
    }
    
    [HttpGet]
    [SwaggerOperation(Summary = "Get all products", Description = "Returns a list of all registered products")]
    [ProducesResponseType(typeof(List<ProductsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result.Value);
    }

}