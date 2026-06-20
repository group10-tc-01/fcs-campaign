using Fcs.Campaign.Application.Abstractions.Messaging;

namespace Fcs.Campaign.Application.UseCases.Campaigns.CreateCampaign;

public sealed record CreateCampaignCommand(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal FinancialGoal) : ICommand<CampaignResponse>;
