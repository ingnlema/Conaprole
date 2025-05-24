using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;


namespace Conaprole.Orders.Application.Distributors.RemoveCategory;

internal sealed class RemoveDistributorCategoryHandler : ICommandHandler<RemoveDistributorCategoryCommand, bool>
{
    private readonly IDistributorRepository _distributorRepository;

    public RemoveDistributorCategoryHandler(IDistributorRepository distributorRepository)
    {
        _distributorRepository = distributorRepository;
    }

    public async Task<Result<bool>> Handle(RemoveDistributorCategoryCommand request, CancellationToken cancellationToken)
    {
        var distributor = await _distributorRepository.GetByPhoneNumberAsync(request.DistributorPhoneNumber, cancellationToken);
        if (distributor is null)
            return Result.Failure<bool>(DistributorErrors.NotFound);

        var removed = distributor.RemoveCategory(request.Category);
        if (!removed)
            return Result.Failure<bool>(DistributorErrors.CategoryNotAssigned);

        return Result.Success(true);
    }
}