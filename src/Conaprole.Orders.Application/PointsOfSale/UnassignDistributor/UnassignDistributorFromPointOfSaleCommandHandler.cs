
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.PointsOfSale;

namespace Conaprole.Orders.Application.PointsOfSale.UnassignDistributor;

internal sealed class UnassignDistributorFromPointOfSaleCommandHandler
    : ICommandHandler<UnassignDistributorFromPointOfSaleCommand, bool>
{
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IDistributorRepository _distributorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnassignDistributorFromPointOfSaleCommandHandler(
        IPointOfSaleRepository pointOfSaleRepository,
        IDistributorRepository distributorRepository,
        IUnitOfWork unitOfWork)
    {
        _pointOfSaleRepository = pointOfSaleRepository;
        _distributorRepository = distributorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(UnassignDistributorFromPointOfSaleCommand request, CancellationToken cancellationToken)
    {
        var pointOfSale = await _pointOfSaleRepository.GetByPhoneNumberAsync(request.PointOfSalePhoneNumber, cancellationToken);

        if (pointOfSale is null)
            return Result.Failure<bool>(PointOfSaleErrors.NotFound);

        var distributor = await _distributorRepository.GetByPhoneNumberAsync(request.DistributorPhoneNumber, cancellationToken);
        if (distributor is null)
            return Result.Failure<bool>(DistributorErrors.NotFound);

        var success = pointOfSale.UnassignDistributor(distributor.Id, request.Category);

        if (!success)
            return Result.Failure<bool>(PointOfSaleErrors.DistributorNotAssigned);

        await _pointOfSaleRepository.UpdateAsync(pointOfSale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}