using Conaprole.Orders.Application.Users.GetAllPermissions;
using Conaprole.Orders.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Conaprole.Orders.Api.Controllers.Users;

[ApiController]
[Route("api/permissions")]
public class PermissionsController : ControllerBase
{
    private readonly ISender _sender;

    public PermissionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    // [HasPermission(Permissions.AdminAccess)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        var query = new GetAllPermissionsQuery();
        var result = await _sender.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        return Ok(result.Value);
    }
}