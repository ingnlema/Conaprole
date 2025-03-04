using Conaprole.Orders.Domain.Abstractions;
using MediatR;

namespace Conaprole.Orders.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}