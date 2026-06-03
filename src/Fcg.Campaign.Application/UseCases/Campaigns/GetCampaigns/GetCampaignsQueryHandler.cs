using Fcg.Campaign.Application.Abstractions.Messaging;
using Fcg.Campaign.Domain;
using Fcg.Campaign.Domain.Campaigns;

namespace Fcg.Campaign.Application.UseCases.Campaigns.GetCampaigns;

public sealed class GetCampaignsQueryHandler : IQueryHandler<GetCampaignsQuery, IReadOnlyList<CampaignResponse>>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<IReadOnlyList<CampaignResponse>>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _campaignRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        return campaigns.Select(CampaignResponse.FromDomain).ToArray();
    }
}
