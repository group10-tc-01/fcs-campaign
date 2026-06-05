using fcs.Campaign.Application.Abstractions.Messaging;

namespace fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;

public sealed record GetDonationEligibilityQuery(Guid CampaignId) : IQuery<GetDonationEligibilityResponse>;
