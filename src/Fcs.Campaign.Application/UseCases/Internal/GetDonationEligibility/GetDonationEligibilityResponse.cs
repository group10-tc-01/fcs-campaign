using System.Diagnostics.CodeAnalysis;
namespace Fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;

[ExcludeFromCodeCoverage]

public sealed record GetDonationEligibilityResponse(Guid CampaignId, bool Eligible, string? Reason);
