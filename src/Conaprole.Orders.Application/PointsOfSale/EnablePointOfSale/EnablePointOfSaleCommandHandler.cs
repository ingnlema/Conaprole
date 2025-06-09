using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.PointsOfSale.EnablePointOfSale;

internal sealed class EnablePointOfSaleCommandHandler : ICommandHandler<EnablePointOfSaleCommand, bool>
{
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnablePointOfSaleCommandHandler(
        IPointOfSaleRepository pointOfSaleRepository,
        IUnitOfWork unitOfWork)
    {
        _pointOfSaleRepository = pointOfSaleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(EnablePointOfSaleCommand request, CancellationToken cancellationToken)
    {
        var pointOfSale = await _pointOfSaleRepository.GetByPhoneNumberAsync(request.PointOfSalePhoneNumber, cancellationToken);
        if (pointOfSale is null)
            return Result.Failure<bool>(PointOfSaleErrors.NotFound);

        if (pointOfSale.IsActive)
            return Result.Failure<bool>(PointOfSaleErrors.AlreadyEnabled);

        pointOfSale.Activate();
        await _pointOfSaleRepository.UpdateAsync(pointOfSale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}