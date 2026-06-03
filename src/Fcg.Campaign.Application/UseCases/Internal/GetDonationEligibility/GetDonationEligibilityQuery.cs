using Fcg.Campaign.Application.Abstractions.Messaging;

namespace Fcg.Campaign.Application.UseCases.Internal.GetDonationEligibility;

public sealed record GetDonationEligibilityQuery(Guid CampaignId) : IQuery<GetDonationEligibilityResponse>;
