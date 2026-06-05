using fcs.Campaign.Application.Abstractions.Messaging;

namespace fcs.Campaign.Application.UseCases.Internal.ProcessDonation;

public sealed record ProcessDonationCommand(Guid CampaignId, Guid DonationId, decimal Amount, DateTime ProcessedAt) : ICommand<ProcessDonationResponse>;
