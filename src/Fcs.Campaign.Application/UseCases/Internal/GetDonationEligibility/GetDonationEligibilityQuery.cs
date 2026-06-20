using Fcs.Campaign.Application.Abstractions.Messaging;

namespace Fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;

public sealed record GetDonationEligibilityQuery(Guid CampaignId) : IQuery<GetDonationEligibilityResponse>;
