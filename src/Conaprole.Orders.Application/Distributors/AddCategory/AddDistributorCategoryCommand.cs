using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Distributors.AddCategory;

public sealed record AddDistributorCategoryCommand(
    string DistributorPhoneNumber,
    Category Category
) : ICommand<bool>;