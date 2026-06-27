using MediatR;

namespace BuildingBlocks.Application;

public interface ICommand<out TResponse> : IRequest<TResponse>;
public interface ICommand : ICommand<Unit>;
public interface IQuery<out TResponse> : IRequest<TResponse>;
