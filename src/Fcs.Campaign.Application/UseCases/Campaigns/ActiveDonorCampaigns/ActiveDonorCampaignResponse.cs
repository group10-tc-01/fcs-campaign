namespace Fcs.Campaign.Application.UseCases.Campaigns.ActiveDonorCampaigns;

public sealed record ActiveDonorCampaignResponse(
    Guid Id,
    string Title,
    decimal FinancialGoal,
    decimal TotalAmountRaised,
    DateTime StartDate,
    DateTime EndDate);
