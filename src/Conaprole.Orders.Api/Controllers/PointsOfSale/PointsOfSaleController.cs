using Conaprole.Orders.Application.PointsOfSale.GetPointOfSaleByPhoneNumber;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Conaprole.Orders.Api.Controllers.PointsOfSale;

[ApiController]
[Route("api/pointsofsale")]
public class PointsOfSaleController : ControllerBase
{
    private readonly ISender _sender;

    public PointsOfSaleController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("by-phone/{phoneNumber}")]
    // [HasPermission(Permissions.PointsOfSaleRead)]
    [SwaggerOperation(Summary = "Get POS by phone number", Description = "Retrieves a point of sale by its phone number. Returns the POS data including its active status for validation purposes.")]
    public async Task<IActionResult> GetPointOfSaleByPhoneNumber(string phoneNumber, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPointOfSaleByPhoneNumberQuery(phoneNumber), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }
}