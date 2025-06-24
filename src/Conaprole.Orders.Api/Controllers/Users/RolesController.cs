using Conaprole.Orders.Application.Users.GetAllRoles;
using Conaprole.Orders.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Conaprole.Orders.Api.Controllers.Users;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Obtiene todos los roles disponibles en el sistema.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.AdminAccess)]
    [SwaggerOperation(Summary = "Obtener todos los roles", Description = "Recupera la lista completa de roles disponibles en el sistema. Solo accesible para administradores.")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var query = new GetAllRolesQuery();
        var result = await _sender.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        return Ok(result.Value);
    }
}