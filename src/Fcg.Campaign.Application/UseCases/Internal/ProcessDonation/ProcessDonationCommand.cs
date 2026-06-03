using Fcg.Campaign.Application.Abstractions.Messaging;

namespace Fcg.Campaign.Application.UseCases.Internal.ProcessDonation;

public sealed record ProcessDonationCommand(Guid CampaignId, Guid DonationId, decimal Amount, DateTime ProcessedAt) : ICommand<ProcessDonationResponse>;
