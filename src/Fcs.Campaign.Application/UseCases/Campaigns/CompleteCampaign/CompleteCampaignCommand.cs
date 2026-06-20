using Fcs.Campaign.Application.Abstractions.Messaging;

namespace Fcs.Campaign.Application.UseCases.Campaigns.CompleteCampaign;

public sealed record CompleteCampaignCommand(Guid Id) : ICommand<CampaignResponse>;
