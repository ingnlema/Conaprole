using Conaprole.Orders.Application.Abstractions.Authentication;
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
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IAuthenticationService authenticationService,
        IUserRepository userRepository,
        IDistributorRepository distributorRepository,
        IUnitOfWork unitOfWork)
    {
        _authenticationService = authenticationService;
        _userRepository = userRepository;
        _distributorRepository = distributorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = User.Create(
            new FirstName(request.FirstName),
            new LastName(request.LastName),
            new Email(request.Email));

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

            user.SetDistributor(distributor.Id);
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