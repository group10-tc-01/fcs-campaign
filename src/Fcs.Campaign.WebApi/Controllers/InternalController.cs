using Fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;
using Fcs.Campaign.Application.UseCases.Internal.ProcessDonation;
using Fcs.Campaign.WebApi.Controllers.v1;
using Fcs.Campaign.WebApi.Extensions;
using Fcs.Campaign.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fcs.Campaign.WebApi.Controllers;

[Route("api/v{version:apiVersion}/internal/campaigns")]
public sealed class InternalController : BaseApiController
{
    public InternalController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("{id:guid}/donation-eligibility")]
    [ProducesResponseType(typeof(ApiResponse<GetDonationEligibilityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDonationEligibility(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDonationEligibilityQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<GetDonationEligibilityResponse>.FromSuccess(result.Value));
    }

    [HttpPost("{id:guid}/donation-processed")]
    [ProducesResponseType(typeof(ApiResponse<ProcessDonationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessDonation(Guid id, [FromBody] ProcessDonationRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ProcessDonationCommand(id, request.DonationId, request.Amount, request.ProcessedAt), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<ProcessDonationResponse>.FromSuccess(result.Value));
    }
}
