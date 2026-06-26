using Fcs.Campaign.Application.UseCases.Campaigns.ActiveDonorCampaigns;
using Fcs.Campaign.WebApi.Extensions;
using Fcs.Campaign.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcs.Campaign.WebApi.Controllers.v1;

[Route("api/v1/campaigns")]
[Authorize(Roles = "Doador")]
public sealed class DonorCampaignsController : BaseApiController
{
    public DonorCampaignsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ActiveDonorCampaignResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetActiveDonorCampaignsQuery(page, pageSize), cancellationToken);

        if (result.IsFailure)
            return result.Error.ToActionResult();

        return Ok(ApiResponse<IReadOnlyList<ActiveDonorCampaignResponse>>.FromSuccess(result.Value));
    }
}
