using Fcs.Campaign.Domain.Results;
using MediatR;

namespace fcs.Campaign.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
