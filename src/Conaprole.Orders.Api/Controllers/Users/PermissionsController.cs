using Conaprole.Orders.Application.Users.GetAllPermissions;
using Conaprole.Orders.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

    /// <summary>
    /// Obtiene todos los permisos disponibles en el sistema.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.AdminAccess)]
    [SwaggerOperation(Summary = "Obtener todos los permisos", Description = "Recupera la lista completa de permisos disponibles en el sistema. Solo accesible para administradores.")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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