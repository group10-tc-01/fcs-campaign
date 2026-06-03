using Fcg.Campaign.Application.UseCases.Campaigns;
using Fcg.Campaign.Application.UseCases.Transparency.GetTransparencyCampaigns;
using Fcg.Campaign.WebApi.Extensions;
using Fcg.Campaign.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Campaign.WebApi.Controllers.v1;

[Route("api/v{version:apiVersion}/transparency/campaigns")]
public sealed class TransparencyController : BaseApiController
{
    public TransparencyController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CampaignResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetTransparencyCampaignsQuery(page, pageSize), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<IReadOnlyList<CampaignResponse>>.FromSuccess(result.Value));
    }
}
