using Conaprole.Orders.Domain.Abstractions;
using MediatR;

namespace Conaprole.Orders.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>, IBaseCommand
{
}

public interface ICommand<TReponse> : IRequest<Result<TReponse>>, IBaseCommand
{
}

public interface IBaseCommand
{
}