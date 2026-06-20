using System.Diagnostics.CodeAnalysis;
namespace Fcs.Campaign.Application.UseCases.Internal.ProcessDonation;

[ExcludeFromCodeCoverage]

public sealed record ProcessDonationResponse(Guid CampaignId, Guid DonationId, bool Processed, bool Duplicate);
