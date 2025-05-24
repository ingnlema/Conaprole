using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Distributors.GetCategories;

public sealed record GetDistributorCategoriesQuery(string DistributorPhoneNumber) : IQuery<List<string>>;