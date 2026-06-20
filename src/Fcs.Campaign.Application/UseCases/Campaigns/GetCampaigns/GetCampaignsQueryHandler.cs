using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Campaigns.GetCampaigns;

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
