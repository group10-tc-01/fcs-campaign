using System.Diagnostics.CodeAnalysis;
namespace fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;

[ExcludeFromCodeCoverage]

public sealed record GetDonationEligibilityResponse(Guid CampaignId, bool Eligible, string? Reason);
