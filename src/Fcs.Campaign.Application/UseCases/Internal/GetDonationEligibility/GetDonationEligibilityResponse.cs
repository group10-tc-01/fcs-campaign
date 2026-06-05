namespace fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;

public sealed record GetDonationEligibilityResponse(Guid CampaignId, bool Eligible, string? Reason);
