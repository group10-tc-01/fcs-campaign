using FluentValidation;

namespace Fcs.Campaign.Application.UseCases.Campaigns.CreateCampaign;

public sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).NotEmpty().MaximumLength(2000);
        RuleFor(command => command.EndDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .Must((command, endDate) => endDate.Date >= command.StartDate.Date)
            .WithMessage("End date cannot be before start date.");
        RuleFor(command => command.FinancialGoal).GreaterThan(0);
    }
}
