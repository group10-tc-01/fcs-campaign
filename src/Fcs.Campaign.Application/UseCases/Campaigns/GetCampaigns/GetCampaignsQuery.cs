using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Common.Pagination;
using Fcs.Campaign.Domain.Campaigns;

namespace Fcs.Campaign.Application.UseCases.Campaigns.GetCampaigns;

public sealed record GetCampaignsQuery(
    int Page = 1,
    int PageSize = 10,
    IReadOnlyCollection<CampaignStatus>? Statuses = null) : IQuery<PagedResponse<CampaignResponse>>;
