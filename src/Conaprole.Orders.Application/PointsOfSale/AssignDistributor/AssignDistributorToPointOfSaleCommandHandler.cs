using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.PointsOfSale.AssignDistributor;

internal sealed class AssignDistributorToPointOfSaleCommandHandler : ICommandHandler<AssignDistributorToPointOfSaleCommand, bool>
{
    private readonly IPointOfSaleRepository _pointOfSaleRepository;
    private readonly IDistributorRepository _distributorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignDistributorToPointOfSaleCommandHandler(
        IPointOfSaleRepository pointOfSaleRepository,
        IDistributorRepository distributorRepository,
        IUnitOfWork unitOfWork)
    {
        _pointOfSaleRepository = pointOfSaleRepository;
        _distributorRepository = distributorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(AssignDistributorToPointOfSaleCommand request, CancellationToken cancellationToken)
    {
        var pointOfSale = await _pointOfSaleRepository.GetByPhoneNumberAsync(request.PointOfSalePhoneNumber, cancellationToken);
        if (pointOfSale is null)
            return Result.Failure<bool>(PointOfSaleErrors.NotFound);

        var distributor = await _distributorRepository.GetByPhoneNumberAsync(request.DistributorPhoneNumber, cancellationToken);
        if (distributor is null)
            return Result.Failure<bool>(DistributorErrors.NotFound);

        // Validate that the distributor supports the requested category
        if (!distributor.SupportedCategories.Contains(request.Category))
            return Result.Failure<bool>(DistributorErrors.CategoryNotSupported);

        var success = pointOfSale.AssignDistributor(distributor.Id, request.Category);
        if (!success)
            return Result.Failure<bool>(PointOfSaleErrors.DistributorAlreadyAssigned);

        await _pointOfSaleRepository.UpdateAsync(pointOfSale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}