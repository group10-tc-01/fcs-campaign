using Fcg.Campaign.Application.Abstractions.Messaging;

namespace Fcg.Campaign.Application.UseCases.Campaigns.UpdateCampaign;

public sealed record UpdateCampaignCommand(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal) : ICommand<CampaignResponse>;
