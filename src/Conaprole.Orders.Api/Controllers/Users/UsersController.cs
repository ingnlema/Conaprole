using Conaprole.Orders.Application.Users.GetLoggedInUser;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Application.Users.RefreshToken;
using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Orders.Application.Users.AssignRole;
using Conaprole.Orders.Application.Users.RemoveRole;
using Conaprole.Orders.Application.Users.GetAllUsers;
using Conaprole.Orders.Application.Users.GetUserRoles;
using Conaprole.Orders.Application.Users.GetUserPermissions;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    
    [HttpGet("me")]
    [HasPermission(Permissions.UsersRead)]
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

    [HttpPost("{userId}/assign-role")]
    // [HasPermission(Permissions.UsersWrite)]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignRoleCommand(userId, request.RoleName);
        var result = await _sender.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{userId}/remove-role")]
    // [HasPermission(Permissions.UsersWrite)]
    public async Task<IActionResult> RemoveRole(Guid userId, [FromBody] RemoveRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new RemoveRoleCommand(userId, request.RoleName);
        var result = await _sender.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpGet]
    // [HasPermission(Permissions.AdminAccess)]
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

    [HttpGet("{userId}/roles")]
    // [HasPermission(Permissions.AdminAccess)]
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

    [HttpGet("{userId}/permissions")]
    // [HasPermission(Permissions.AdminAccess)]
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

}