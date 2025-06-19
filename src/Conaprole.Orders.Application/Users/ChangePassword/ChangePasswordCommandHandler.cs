using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IAuthorizationService _authorizationService;

    public ChangePasswordCommandHandler(
        IAuthenticationService authenticationService,
        IUserRepository userRepository,
        IUserContext userContext,
        IAuthorizationService authorizationService)
    {
        _authenticationService = authenticationService;
        _userRepository = userRepository;
        _userContext = userContext;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        // Authorization logic: user can change their own password, or user with users:write permission can change any password
        var currentUserId = _userContext.UserId;
        var isOwnPassword = currentUserId == request.UserId;
        
        if (!isOwnPassword)
        {
            // Check if user has users:write permission for changing other users' passwords
            var permissions = await _authorizationService.GetPermissionsForUserAsync(_userContext.IdentityId);
            
            if (!permissions.Contains("users:write"))
            {
                return Result.Failure(new Error(
                    "Authorization.Insufficient", 
                    "You don't have permission to change other users' passwords"));
            }
        }

        await _authenticationService.ChangePasswordAsync(
            user.IdentityId,
            request.NewPassword,
            cancellationToken);

        return Result.Success();
    }
}