using Fcs.Campaign.Domain.Results;
using MediatR;

namespace Fcs.Campaign.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
