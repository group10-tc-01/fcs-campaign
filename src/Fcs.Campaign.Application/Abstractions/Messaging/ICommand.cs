using Fcs.Campaign.Domain.Results;
using MediatR;

namespace fcs.Campaign.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
