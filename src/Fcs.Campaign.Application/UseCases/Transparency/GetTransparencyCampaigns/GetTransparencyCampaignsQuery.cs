using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Common.Pagination;

namespace Fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

public sealed record GetTransparencyCampaignsQuery(
    int Page = 1,
    int PageSize = 10) : IQuery<PagedResponse<TransparencyCampaignResponse>>;
