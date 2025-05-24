using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Abstractions;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Distributors.AddCategory;

internal sealed class AddDistributorCategoryCommandHandler
    : ICommandHandler<AddDistributorCategoryCommand, bool>
{
    private readonly IDistributorRepository _distributorRepository;

    public AddDistributorCategoryCommandHandler(IDistributorRepository distributorRepository)
    {
        _distributorRepository = distributorRepository;
    }

    public async Task<Result<bool>> Handle(AddDistributorCategoryCommand request, CancellationToken cancellationToken)
    {
        var distributor = await _distributorRepository.GetByPhoneNumberAsync(request.DistributorPhoneNumber, cancellationToken);
        if (distributor is null)
            return Result.Failure<bool>(DistributorErrors.NotFound);

        var added = distributor.AddCategory(request.Category);
        if (!added)
            return Result.Failure<bool>(DistributorErrors.CategoryAlreadyAssigned);

        return Result.Success(true);
    }
}