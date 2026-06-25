using Fcs.Campaign.Application.Abstractions.Messaging;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.Domain.Results;

namespace Fcs.Campaign.Application.UseCases.Campaigns.ActiveDonorCampaigns;

public sealed class GetActiveDonorCampaignsQueryHandler
    : IQueryHandler<GetActiveDonorCampaignsQuery, IReadOnlyList<ActiveDonorCampaignResponse>>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetActiveDonorCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<Result<IReadOnlyList<ActiveDonorCampaignResponse>>> Handle(
        GetActiveDonorCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _campaignRepository.GetAllActiveAsync(request.Page, request.PageSize, cancellationToken);
        return Result.Success<IReadOnlyList<ActiveDonorCampaignResponse>>(
            campaigns
                .Select(c => new ActiveDonorCampaignResponse(
                    c.Id, c.Title, c.FinancialGoal, c.TotalAmountRaised, c.StartDate, c.EndDate))
                .ToArray());
    }
}
