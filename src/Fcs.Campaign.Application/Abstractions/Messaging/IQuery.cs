using Fcs.Campaign.Domain.Results;
using MediatR;

namespace fcs.Campaign.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
