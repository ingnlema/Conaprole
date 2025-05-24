using System.Collections.Generic;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Application.Abstractions.Messaging;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.Distributors.CreateDistributor;

public sealed record CreateDistributorCommand(
    string Name,
    string PhoneNumber,
    string Address,
    List<Category> Categories
) : ICommand<Guid>;