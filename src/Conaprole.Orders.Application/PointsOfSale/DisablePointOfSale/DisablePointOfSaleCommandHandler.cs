using Conaprole.Orders.Application.Abstractions.Clock;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.PointsOfSale.DisablePointOfSale;

internal sealed class DisablePointOfSaleCommandHandler : ICommandHandler<DisablePointOfSaleCommand, bool>
{
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DisablePointOfSaleCommandHandler(
        IPointOfSaleRepository pointOfSaleRepository,
        IUnitOfWork unitOfWork)
    {
        _pointOfSaleRepository = pointOfSaleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DisablePointOfSaleCommand request, CancellationToken cancellationToken)
    {
        var pointOfSale = await _pointOfSaleRepository.GetByPhoneNumberAsync(request.PointOfSalePhoneNumber, cancellationToken);
        if (pointOfSale is null)
            return Result.Failure<bool>(PointOfSaleErrors.NotFound);

        if (!pointOfSale.IsActive)
            return Result.Failure<bool>(PointOfSaleErrors.AlreadyDisabled);

        pointOfSale.Deactivate();
        await _pointOfSaleRepository.UpdateAsync(pointOfSale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}