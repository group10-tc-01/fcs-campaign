using fcs.Campaign.Application.Abstractions.Messaging;

namespace fcs.Campaign.Application.UseCases.Campaigns.GetCampaigns;

public sealed record GetCampaignsQuery(int Page = 1, int PageSize = 10) : IQuery<IReadOnlyList<CampaignResponse>>;
