using Conaprole.Orders.Application.Products.GetProduct;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conaprole.Orders.Api.Controllers.Products;

[Authorize]
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

}