using fcs.Campaign.Application.Abstractions.Messaging;

namespace fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

public sealed record GetTransparencyCampaignsQuery(int Page = 1, int PageSize = 10) : IQuery<IReadOnlyList<TransparencyCampaignResponse>>;
