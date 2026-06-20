using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

public sealed class GetTransparencyCampaignsQueryHandler : IQueryHandler<GetTransparencyCampaignsQuery, IReadOnlyList<TransparencyCampaignResponse>>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetTransparencyCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<IReadOnlyList<TransparencyCampaignResponse>>> Handle(GetTransparencyCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _campaignRepository.GetAllActiveAsync(request.Page, request.PageSize, cancellationToken);
        return campaigns.Select(TransparencyCampaignResponse.FromDomain).ToArray();
    }
}
