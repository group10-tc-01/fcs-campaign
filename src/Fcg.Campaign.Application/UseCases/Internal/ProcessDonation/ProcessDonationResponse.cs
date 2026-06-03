namespace Fcg.Campaign.Application.UseCases.Internal.ProcessDonation;

public sealed record ProcessDonationResponse(Guid CampaignId, Guid DonationId, bool Processed, bool Duplicate);
