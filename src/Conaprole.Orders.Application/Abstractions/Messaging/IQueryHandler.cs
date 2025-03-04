using Conaprole.Orders.Domain.Abstractions;
using MediatR;

namespace Conaprole.Orders.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}