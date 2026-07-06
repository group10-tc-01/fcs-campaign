using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Common.Pagination;

namespace Fcs.Campaign.Application.UseCases.Campaigns.ActiveDonorCampaigns;

public sealed record GetActiveDonorCampaignsQuery(int Page = 1, int PageSize = 10)
    : IQuery<PagedResponse<ActiveDonorCampaignResponse>>;
