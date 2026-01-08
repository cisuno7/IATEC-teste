using MediatR;

namespace AgendaManager.Application.Interfaces;

public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IRequest<TResult>
{
}
