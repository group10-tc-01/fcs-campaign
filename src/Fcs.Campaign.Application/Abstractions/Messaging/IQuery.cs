using MediatR;
using fcs.Campaign.Domain;

namespace fcs.Campaign.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
