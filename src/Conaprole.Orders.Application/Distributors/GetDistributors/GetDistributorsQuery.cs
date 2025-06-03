using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Distributors.GetDistributors;

public sealed record GetDistributorsQuery() : IQuery<List<DistributorSummaryResponse>>;