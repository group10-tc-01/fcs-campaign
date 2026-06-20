using Fcs.Campaign.Application.Abstractions.Messaging;

namespace Fcs.Campaign.Application.UseCases.Campaigns.CancelCampaign;

public sealed record CancelCampaignCommand(Guid Id) : ICommand<CampaignResponse>;
