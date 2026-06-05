using MediatR;
using fcs.Campaign.Domain;

namespace fcs.Campaign.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
