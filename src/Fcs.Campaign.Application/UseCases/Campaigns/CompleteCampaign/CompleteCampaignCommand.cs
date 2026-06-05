using fcs.Campaign.Application.Abstractions.Messaging;

namespace fcs.Campaign.Application.UseCases.Campaigns.CompleteCampaign;

public sealed record CompleteCampaignCommand(Guid Id) : ICommand<CampaignResponse>;
