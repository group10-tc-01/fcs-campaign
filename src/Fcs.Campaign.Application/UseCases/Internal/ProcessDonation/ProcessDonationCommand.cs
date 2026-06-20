using Fcs.Campaign.Application.Abstractions.Messaging;

namespace Fcs.Campaign.Application.UseCases.Internal.ProcessDonation;

public sealed record ProcessDonationCommand(Guid CampaignId, Guid DonationId, decimal Amount, DateTime ProcessedAt) : ICommand<ProcessDonationResponse>;
