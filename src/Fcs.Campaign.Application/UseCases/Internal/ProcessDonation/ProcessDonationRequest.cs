namespace Fcs.Campaign.Application.UseCases.Internal.ProcessDonation;

public sealed record ProcessDonationRequest(
    Guid DonationId,
    decimal Amount,
    DateTime ProcessedAt);
