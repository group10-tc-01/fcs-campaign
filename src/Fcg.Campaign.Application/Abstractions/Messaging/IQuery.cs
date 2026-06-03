using MediatR;
using Fcg.Campaign.Domain;

namespace Fcg.Campaign.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
