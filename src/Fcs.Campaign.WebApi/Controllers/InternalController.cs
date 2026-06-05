using fcs.Campaign.Application.UseCases.Internal.GetDonationEligibility;
using fcs.Campaign.Application.UseCases.Internal.ProcessDonation;
using fcs.Campaign.WebApi.Extensions;
using fcs.Campaign.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace fcs.Campaign.WebApi.Controllers;

[ApiController]
[Route("internal/campaigns")]
public sealed class InternalController : ControllerBase
{
    private readonly IMediator _mediator;

    public InternalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}/donation-eligibility")]
    [ProducesResponseType(typeof(ApiResponse<GetDonationEligibilityResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDonationEligibility(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDonationEligibilityQuery(id), cancellationToken);
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
        var result = await _mediator.Send(new ProcessDonationCommand(id, request.DonationId, request.Amount, request.ProcessedAt), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<ProcessDonationResponse>.FromSuccess(result.Value));
    }

    public sealed record ProcessDonationRequest(Guid DonationId, decimal Amount, DateTime ProcessedAt);
}
