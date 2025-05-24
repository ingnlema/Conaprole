using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;


namespace Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;

internal sealed class CreatePointOfSaleCommandHandler : ICommandHandler<CreatePointOfSaleCommand, Guid>
{
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreatePointOfSaleCommandHandler(
        IPointOfSaleRepository pointOfSaleRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _pointOfSaleRepository = pointOfSaleRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(CreatePointOfSaleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _pointOfSaleRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<Guid>(PointOfSaleErrors.AlreadyExists);
        }

        var pointOfSale = new PointOfSale(
            Guid.NewGuid(),
            request.Name,
            request.PhoneNumber,
            new Address(request.City, request.Street, request.ZipCode),
            _dateTimeProvider.UtcNow
        );

        await _pointOfSaleRepository.AddAsync(pointOfSale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(pointOfSale.Id);
    }
}