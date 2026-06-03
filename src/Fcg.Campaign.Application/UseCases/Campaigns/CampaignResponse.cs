using Fcg.Campaign.Domain.Campaigns;

namespace Fcg.Campaign.Application.UseCases.Campaigns;

public sealed record CampaignResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal,
    CampaignStatus Status,
    decimal TotalAmountRaised,
    Guid CreatedByManagerId,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static CampaignResponse FromDomain(Domain.Campaigns.Campaign campaign) =>
        new(
            campaign.Id,
            campaign.Title,
            campaign.Description,
            campaign.StartDate,
            campaign.EndDate,
            campaign.FinancialGoal,
            campaign.Status,
            campaign.TotalAmountRaised,
            campaign.CreatedByManagerId,
            campaign.CreatedAt,
            campaign.UpdatedAt);
}
