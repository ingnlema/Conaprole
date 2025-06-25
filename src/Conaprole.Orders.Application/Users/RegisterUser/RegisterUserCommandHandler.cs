using Conaprole.Orders.Application.Abstractions.Authentication;
using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Application.Users.RegisterUser;

internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserRepository _userRepository;
    private readonly IDistributorRepository _distributorRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterUserCommandHandler(
        IAuthenticationService authenticationService,
        IUserRepository userRepository,
        IDistributorRepository distributorRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _authenticationService = authenticationService;
        _userRepository = userRepository;
        _distributorRepository = distributorRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = User.Create(
            new FirstName(request.FirstName),
            new LastName(request.LastName),
            new Email(request.Email),
            _dateTimeProvider.UtcNow);

        // Assign the default Registered role to all users
        var registeredRole = await _roleRepository.GetByNameAsync("Registered", cancellationToken);
        if (registeredRole is null)
        {
            return Result.Failure<Guid>(new Error("Role.NotFound", "The 'Registered' role was not found."));
        }
        user.AssignRole(registeredRole);

        // If distributor phone number is provided, try to associate the distributor
        if (!string.IsNullOrEmpty(request.DistributorPhoneNumber))
        {
            var distributor = await _distributorRepository.GetByPhoneNumberAsync(
                request.DistributorPhoneNumber,
                cancellationToken);

            if (distributor is null)
            {
                return Result.Failure<Guid>(DistributorErrors.NotFound);
            }

            var distributorRole = await _roleRepository.GetByNameAsync("Distributor", cancellationToken);
            if (distributorRole is null)
            {
                return Result.Failure<Guid>(new Error("Role.NotFound", "The 'Distributor' role was not found."));
            }

            user.SetDistributor(distributor.Id);
            user.AssignRole(distributorRole);
        }

        var identityId = await _authenticationService.RegisterAsync(
            user,
            request.Password,
            cancellationToken);

        user.SetIdentityId(identityId);

        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success( user.Id);
    }
}