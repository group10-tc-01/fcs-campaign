using Fcg.Campaign.Application.Abstractions.Messaging;

namespace Fcg.Campaign.Application.UseCases.Campaigns.CreateCampaign;

public sealed record CreateCampaignCommand(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal) : ICommand<CampaignResponse>;
