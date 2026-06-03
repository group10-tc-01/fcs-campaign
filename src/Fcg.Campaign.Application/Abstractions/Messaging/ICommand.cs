using MediatR;
using Fcg.Campaign.Domain;

namespace Fcg.Campaign.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
