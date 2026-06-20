using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Common.Pagination;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;

public sealed class GetTransparencyCampaignsQueryHandler : IQueryHandler<GetTransparencyCampaignsQuery, PagedResponse<TransparencyCampaignResponse>>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetTransparencyCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<PagedResponse<TransparencyCampaignResponse>>> Handle(GetTransparencyCampaignsQuery request, CancellationToken cancellationToken)
    {
        var page = PagedResponse<TransparencyCampaignResponse>.NormalizePage(request.Page);
        var pageSize = PagedResponse<TransparencyCampaignResponse>.NormalizePageSize(request.PageSize);
        var campaigns = await _campaignRepository.GetAllActiveAsync(page, pageSize, cancellationToken);
        var totalCount = await _campaignRepository.CountActiveAsync(cancellationToken);

        return new PagedResponse<TransparencyCampaignResponse>(
            campaigns.Select(TransparencyCampaignResponse.FromDomain).ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
