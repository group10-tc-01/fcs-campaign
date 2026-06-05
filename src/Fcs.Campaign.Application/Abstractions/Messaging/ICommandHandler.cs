using fcs.Campaign.Domain;
using MediatR;

namespace fcs.Campaign.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
