using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Application.Common.Pagination;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Campaigns.ActiveDonorCampaigns;

public sealed class GetActiveDonorCampaignsQueryHandler
    : IQueryHandler<GetActiveDonorCampaignsQuery, PagedResponse<ActiveDonorCampaignResponse>>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetActiveDonorCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<PagedResponse<ActiveDonorCampaignResponse>>> Handle(
        GetActiveDonorCampaignsQuery request, CancellationToken cancellationToken)
    {
        var page = PagedResponse<ActiveDonorCampaignResponse>.NormalizePage(request.Page);
        var pageSize = PagedResponse<ActiveDonorCampaignResponse>.NormalizePageSize(request.PageSize);

        var campaigns = await _campaignRepository.GetAllActiveAsync(page, pageSize, cancellationToken);
        var totalCount = await _campaignRepository.CountActiveAsync(cancellationToken);

        return new PagedResponse<ActiveDonorCampaignResponse>(
            campaigns
                .Select(c => new ActiveDonorCampaignResponse(
                    c.Id, c.Title, c.FinancialGoal, c.TotalAmountRaised, c.StartDate, c.EndDate))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
