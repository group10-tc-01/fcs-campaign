using fcs.Campaign.Application.Abstractions.Messaging;
using fcs.Campaign.Application.UseCases.Campaigns;
using fcs.Campaign.Domain;
using fcs.Campaign.Domain.Campaigns;

namespace fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

public sealed class GetTransparencyCampaignsQueryHandler : IQueryHandler<GetTransparencyCampaignsQuery, IReadOnlyList<CampaignResponse>>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetTransparencyCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<IReadOnlyList<CampaignResponse>>> Handle(GetTransparencyCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _campaignRepository.GetAllActiveAsync(request.Page, request.PageSize, cancellationToken);
        return campaigns.Select(CampaignResponse.FromDomain).ToArray();
    }
}
