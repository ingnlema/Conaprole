using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Distributors.RemoveCategory;

public sealed record RemoveDistributorCategoryCommand(
    string DistributorPhoneNumber,
    Category Category
) : ICommand<bool>;