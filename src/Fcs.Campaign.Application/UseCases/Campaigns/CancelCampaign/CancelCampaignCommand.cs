using fcs.Campaign.Application.Abstractions.Messaging;

namespace fcs.Campaign.Application.UseCases.Campaigns.CancelCampaign;

public sealed record CancelCampaignCommand(Guid Id) : ICommand<CampaignResponse>;
