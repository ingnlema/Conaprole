using Conaprole.Orders.Application.Users.GetLoggedInUser;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Application.Users.RefreshToken;
using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Orders.Application.Users.AssignRole;
using Conaprole.Orders.Application.Users.RemoveRole;
using Conaprole.Orders.Application.Users.GetAllUsers;
using Conaprole.Orders.Application.Users.GetUserRoles;
using Conaprole.Orders.Application.Users.GetUserPermissions;
using Conaprole.Orders.Application.Users.ChangePassword;
using Conaprole.Orders.Application.Users.DeleteUser;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Conaprole.Orders.Api.Controllers.Users;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }
    
    /// <summary>
    /// Obtiene la información del usuario logueado actualmente.
    /// </summary>
    [HttpGet("me")]
    [HasPermission(Permissions.UsersRead)]
    [SwaggerOperation(Summary = "Obtener información del usuario actual", Description = "Recupera los datos del usuario que está actualmente autenticado en el sistema.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLoggedInUser(CancellationToken cancellationToken)
    {
        var query = new GetLoggedInUserQuery();

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    // [HasPermission(Permissions.UsersWrite)] // Public endpoint - no permission needed
    public async Task<IActionResult> Register(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Password,
            request.DistributorPhoneNumber);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    
    [AllowAnonymous]
    [HttpPost("login")]
    // [HasPermission(Permissions.UsersRead)] // Public endpoint - no permission needed
    public async Task<IActionResult> LogIn(
        LogInUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogInUserCommand(request.Email, request.Password);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    // [HasPermission(Permissions.UsersRead)] // Public endpoint - no permission needed
    public async Task<IActionResult> RefreshToken(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Asigna un rol específico a un usuario.
    /// </summary>
    [HttpPost("{userId}/assign-role")]
    [HasPermission(Permissions.UsersWrite)]
    [SwaggerOperation(Summary = "Asignar rol a usuario", Description = "Asigna un rol específico a un usuario del sistema. Requiere permisos de escritura de usuarios.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignRoleCommand(userId, request.RoleName);
        var result = await _sender.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Remueve un rol específico de un usuario.
    /// </summary>
    [HttpPost("{userId}/remove-role")]
    [HasPermission(Permissions.UsersWrite)]
    [SwaggerOperation(Summary = "Remover rol de usuario", Description = "Remueve un rol específico de un usuario del sistema. Requiere permisos de escritura de usuarios.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRole(Guid userId, [FromBody] RemoveRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new RemoveRoleCommand(userId, request.RoleName);
        var result = await _sender.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Obtiene todos los usuarios del sistema con filtro opcional por rol.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.AdminAccess)]
    [SwaggerOperation(Summary = "Obtener todos los usuarios", Description = "Recupera la lista de todos los usuarios del sistema, con opción de filtrar por rol específico. Solo accesible para administradores.")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? roleFilter, CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery(roleFilter);
        var result = await _sender.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene los roles asignados a un usuario específico.
    /// </summary>
    [HttpGet("{userId}/roles")]
    [HasPermission(Permissions.AdminAccess)]
    [SwaggerOperation(Summary = "Obtener roles de usuario", Description = "Recupera la lista de roles asignados a un usuario específico. Solo accesible para administradores.")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetUserRolesQuery(userId);
        var result = await _sender.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene los permisos efectivos de un usuario específico.
    /// </summary>
    [HttpGet("{userId}/permissions")]
    [HasPermission(Permissions.AdminAccess)]
    [SwaggerOperation(Summary = "Obtener permisos de usuario", Description = "Recupera la lista de permisos efectivos de un usuario específico basado en sus roles asignados. Solo accesible para administradores.")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetUserPermissionsQuery(userId);
        var result = await _sender.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Cambia la contraseña de un usuario.
    /// </summary>
    [HttpPut("{userId}/change-password")]
    [HasPermission(Permissions.UsersWrite)]
    [SwaggerOperation(Summary = "Cambiar contraseña de usuario", Description = "Permite cambiar la contraseña de un usuario. Requiere permisos de escritura de usuarios.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangePassword(
        Guid userId, 
        [FromBody] ChangePasswordRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(userId, request.NewPassword);
        var result = await _sender.Send(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        return NoContent();
    }

    [HttpDelete("{userId}")]
    [HasPermission(Permissions.AdminAccess)]
    public async Task<IActionResult> DeleteUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(userId);
        var result = await _sender.Send(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        return NoContent();
    }

}