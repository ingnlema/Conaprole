using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Distributors.CreateDistributor;

public sealed class CreateDistributorCommandHandler : ICommandHandler<CreateDistributorCommand, Guid>
{
    private readonly IDistributorRepository _distributorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateDistributorCommandHandler(
        IDistributorRepository distributorRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _distributorRepository = distributorRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateDistributorCommand request, CancellationToken cancellationToken)
    {
        var existing = await _distributorRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(DistributorErrors.AlreadyExists);

        var distributor = new Distributor(
            Guid.NewGuid(),
            request.Name,
            request.PhoneNumber,
            request.Address,
            _dateTimeProvider.UtcNow,
            request.Categories
        );

        await _distributorRepository.AddAsync(distributor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(distributor.Id);
    }
}