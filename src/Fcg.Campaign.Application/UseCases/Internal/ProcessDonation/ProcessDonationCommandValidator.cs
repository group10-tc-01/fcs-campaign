using FluentValidation;

namespace Fcg.Campaign.Application.UseCases.Internal.ProcessDonation;

public sealed class ProcessDonationCommandValidator : AbstractValidator<ProcessDonationCommand>
{
    public ProcessDonationCommandValidator()
    {
        RuleFor(command => command.CampaignId).NotEmpty();
        RuleFor(command => command.DonationId).NotEmpty();
        RuleFor(command => command.Amount).GreaterThan(0);
    }
}
