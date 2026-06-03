using Fcg.Campaign.Application.Abstractions.Messaging;

namespace Fcg.Campaign.Application.UseCases.Campaigns.CompleteCampaign;

public sealed record CompleteCampaignCommand(Guid Id) : ICommand<CampaignResponse>;
