using fcs.Campaign.Application.Abstractions.Messaging;
using fcs.Campaign.Application.UseCases.Campaigns;

namespace fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

public sealed record GetTransparencyCampaignsQuery(int Page = 1, int PageSize = 10) : IQuery<IReadOnlyList<CampaignResponse>>;
