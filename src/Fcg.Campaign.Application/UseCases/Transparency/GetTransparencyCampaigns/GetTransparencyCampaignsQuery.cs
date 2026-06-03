using Fcg.Campaign.Application.Abstractions.Messaging;
using Fcg.Campaign.Application.UseCases.Campaigns;

namespace Fcg.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

public sealed record GetTransparencyCampaignsQuery(int Page = 1, int PageSize = 10) : IQuery<IReadOnlyList<CampaignResponse>>;
