using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.GetAllUsers;

public sealed record GetAllUsersQuery(string? RoleFilter = null) : IQuery<IReadOnlyList<UserSummaryResponse>>;