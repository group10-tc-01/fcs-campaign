namespace Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaign;

public sealed record UpdateCampaignRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal);
