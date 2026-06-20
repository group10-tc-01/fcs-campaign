using System.Diagnostics.CodeAnalysis;

namespace Fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

[ExcludeFromCodeCoverage]
public sealed record TransparencyCampaignResponse(
    string Title,
    decimal FinancialGoal,
    decimal TotalAmountRaised)
{
    public static TransparencyCampaignResponse FromDomain(Domain.Campaigns.Campaign campaign) =>
        new(campaign.Title, campaign.FinancialGoal, campaign.TotalAmountRaised);
}
