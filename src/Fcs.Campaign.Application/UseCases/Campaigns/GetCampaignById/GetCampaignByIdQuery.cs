using Fcs.Campaign.Application.Abstractions.Messaging;

namespace Fcs.Campaign.Application.UseCases.Campaigns.GetCampaignById;

public sealed record GetCampaignByIdQuery(Guid Id) : IQuery<CampaignResponse>;
