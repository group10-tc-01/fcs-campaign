using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Campaigns.GetCampaignById;

public sealed class GetCampaignByIdQueryHandler : IQueryHandler<GetCampaignByIdQuery, CampaignResponse>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetCampaignByIdQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<CampaignResponse>> Handle(
        GetCampaignByIdQuery request,
        CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.Id, cancellationToken);
        return campaign is null
            ? Error.NotFound("Campaign.NotFound", "Campaign was not found.")
            : CampaignResponse.FromDomain(campaign);
    }
}
