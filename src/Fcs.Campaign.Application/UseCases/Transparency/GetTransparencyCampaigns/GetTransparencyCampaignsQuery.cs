using Fcs.Campaign.Application.Abstractions.Messaging;

namespace Fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

public sealed record GetTransparencyCampaignsQuery(int Page = 1, int PageSize = 10) : IQuery<IReadOnlyList<TransparencyCampaignResponse>>;
