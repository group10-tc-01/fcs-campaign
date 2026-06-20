using Fcs.Campaign.Application.Abstractions.Messaging;
namespace Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaignStatus;

public sealed record UpdateCampaignStatusCommand(
    Guid Id,
    string Status) : ICommand<CampaignStatusResponse>;
