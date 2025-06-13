using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Application.Users.RemoveRole;

internal sealed class RemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
        if (role is null)
        {
            return Result.Failure(new Error("Role.NotFound", $"Role '{request.RoleName}' not found."));
        }

        user.RemoveRole(role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

}