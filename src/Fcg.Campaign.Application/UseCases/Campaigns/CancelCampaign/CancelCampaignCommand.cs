using Fcg.Campaign.Application.Abstractions.Messaging;

namespace Fcg.Campaign.Application.UseCases.Campaigns.CancelCampaign;

public sealed record CancelCampaignCommand(Guid Id) : ICommand<CampaignResponse>;
