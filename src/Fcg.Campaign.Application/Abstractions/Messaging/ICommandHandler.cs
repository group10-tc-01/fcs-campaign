using Fcg.Campaign.Domain;
using MediatR;

namespace Fcg.Campaign.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
