using FluentValidation;

namespace Fcg.Campaign.Application.UseCases.Campaigns.UpdateCampaign;

public sealed class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(2000);
        RuleFor(command => command.EndDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date);
        RuleFor(command => command.FinancialGoal).GreaterThan(0);
    }
}
