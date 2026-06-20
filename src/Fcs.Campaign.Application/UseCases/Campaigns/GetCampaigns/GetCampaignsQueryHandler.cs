using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Common.Pagination;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Campaigns.GetCampaigns;

public sealed class GetCampaignsQueryHandler : IQueryHandler<GetCampaignsQuery, PagedResponse<CampaignResponse>>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<PagedResponse<CampaignResponse>>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var page = PagedResponse<CampaignResponse>.NormalizePage(request.Page);
        var pageSize = PagedResponse<CampaignResponse>.NormalizePageSize(request.PageSize);
        var statuses = request.Statuses?
            .Distinct()
            .ToArray();

        var campaigns = await _campaignRepository.GetAllAsync(page, pageSize, statuses, cancellationToken);
        var totalCount = await _campaignRepository.CountAsync(statuses, cancellationToken);

        return new PagedResponse<CampaignResponse>(
            campaigns.Select(CampaignResponse.FromDomain).ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
