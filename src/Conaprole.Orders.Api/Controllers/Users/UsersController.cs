using Conaprole.Orders.Application.Users.GetLoggedInUser;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Orders.Application.Users.AssignRole;
using Conaprole.Orders.Application.Users.RemoveRole;
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

}