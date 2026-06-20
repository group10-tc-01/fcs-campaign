using Fcs.Campaign.Domain.Campaigns;

namespace Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaignStatus;

public sealed record CampaignStatusResponse(
    Guid Id,
    CampaignStatus Status,
    DateTime? UpdatedAt);
