using Fcg.Campaign.Application.Abstractions.Messaging;
using Fcg.Campaign.Application.UseCases.Campaigns;
using Fcg.Campaign.Domain;
using Fcg.Campaign.Domain.Campaigns;

namespace Fcg.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

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
