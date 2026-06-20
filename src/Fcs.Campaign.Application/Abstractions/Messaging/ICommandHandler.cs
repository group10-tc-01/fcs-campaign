using Fcs.Campaign.Domain.Results;
using MediatR;

namespace Fcs.Campaign.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
