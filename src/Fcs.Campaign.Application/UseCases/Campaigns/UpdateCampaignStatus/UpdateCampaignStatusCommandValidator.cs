using Fcs.Campaign.Domain.Campaigns;
using FluentValidation;

namespace Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaignStatus;

public sealed class UpdateCampaignStatusCommandValidator : AbstractValidator<UpdateCampaignStatusCommand>
{
    private const string ValidStatusMessage = "Status must be one of: Completed, Canceled.";

    public UpdateCampaignStatusCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Status)
            .NotEmpty()
            .Must(BeValidStatus)
            .WithMessage(ValidStatusMessage);
    }

    private static bool BeValidStatus(string status)
    {
        return Enum.TryParse<CampaignStatus>(status, ignoreCase: true, out var parsedStatus)
            && parsedStatus is CampaignStatus.Completed or CampaignStatus.Canceled;
    }
}
