using MediatR;

namespace AgendaManager.Application.Interfaces;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, bool>
    where TCommand : IRequest<bool>
{
}

public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : IRequest<TResult>
{
}
