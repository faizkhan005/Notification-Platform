using MediatR;

namespace BuildingBlocks.Application;

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;

public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, Unit>
    where TCommand : ICommand;

public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
