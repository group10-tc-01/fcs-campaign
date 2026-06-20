using fcs.Campaign.Application.Abstractions.Messaging;
using fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;

public sealed class GetDonationEligibilityQueryHandler : IQueryHandler<GetDonationEligibilityQuery, GetDonationEligibilityResponse>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetDonationEligibilityQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<GetDonationEligibilityResponse>> Handle(GetDonationEligibilityQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Error.NotFound("Campaign.NotFound", "Campaign was not found.");
        }

        var eligible = campaign.Status == CampaignStatus.Active;
        return new GetDonationEligibilityResponse(campaign.Id, eligible, eligible ? null : $"Campaign status is {campaign.Status}.");
    }
}
