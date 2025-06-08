using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Application.Users.RemoveRole;

internal sealed class RemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var role = GetRoleByName(request.RoleName);
        if (role is null)
        {
            return Result.Failure(new Error("Role.NotFound", $"Role '{request.RoleName}' not found."));
        }

        user.RemoveRole(role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Role? GetRoleByName(string roleName)
    {
        return roleName switch
        {
            "Registered" => Role.Registered,
            "API" => Role.API,
            "Administrator" => Role.Administrator,
            "Distributor" => Role.Distributor,
            _ => null
        };
    }
}