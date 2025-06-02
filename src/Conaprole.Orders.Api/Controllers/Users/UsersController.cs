using Conaprole.Orders.Application.Users.GetLoggedInUser;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Orders.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Conaprole.Orders.Domain.Abstractions;

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
    /// Gets the currently logged in user information.
    /// </summary>
    [HttpGet("me")]
    [HasPermission(Permissions.UsersRead)]
    [SwaggerOperation(Summary = "Get logged in user", Description = "Returns the information of the currently authenticated user.")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLoggedInUser(CancellationToken cancellationToken)
    {
        var query = new GetLoggedInUserQuery();

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <remarks>
    /// Creates a new user account with email, name, password and optional distributor association.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Register new user", Description = "Creates a new user account with email, personal information and optional distributor association.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Authenticates a user and returns access token.
    /// </summary>
    /// <remarks>
    /// Validates user credentials and returns JWT token for API access.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation(Summary = "User login", Description = "Authenticates user credentials and returns access token for API access.")]
    [ProducesResponseType(typeof(AccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
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

}