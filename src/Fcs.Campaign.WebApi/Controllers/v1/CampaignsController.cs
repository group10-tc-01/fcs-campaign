using Fcs.Campaign.Application.Common.Pagination;
using Fcs.Campaign.Application.UseCases.Campaigns;
using Fcs.Campaign.Application.UseCases.Campaigns.CreateCampaign;
using Fcs.Campaign.Application.UseCases.Campaigns.GetCampaignById;
using Fcs.Campaign.Application.UseCases.Campaigns.GetCampaigns;
using Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaign;
using Fcs.Campaign.Application.UseCases.Campaigns.UpdateCampaignStatus;
using Fcs.Campaign.Domain.Campaigns;
using Fcs.Campaign.WebApi.Extensions;
using Fcs.Campaign.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcs.Campaign.WebApi.Controllers.v1;

[Authorize(Roles = "GestorONG")]
public sealed class CampaignsController : BaseApiController
{
    public CampaignsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CampaignResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Created($"/api/v1/campaigns/{result.Value.Id}", ApiResponse<CampaignResponse>.FromSuccess(result.Value));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CampaignResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateCampaignCommand(id, request.Title, request.Description, request.StartDate, request.EndDate, request.FinancialGoal), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<CampaignResponse>.FromSuccess(result.Value));
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<CampaignStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(Guid id,[FromBody] UpdateCampaignStatusRequest request,CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateCampaignStatusCommand(id, request.Status), cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<CampaignStatusResponse>.FromSuccess(result.Value));
    }

    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<CampaignStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new UpdateCampaignStatusCommand(id, CampaignStatus.Completed.ToString()), cancellationToken);

        if (result.IsFailure)
            return result.Error.ToActionResult();

        return Ok(ApiResponse<CampaignStatusResponse>.FromSuccess(result.Value));
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<CampaignStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new UpdateCampaignStatusCommand(id, CampaignStatus.Canceled.ToString()), cancellationToken);

        if (result.IsFailure)
            return result.Error.ToActionResult();

        return Ok(ApiResponse<CampaignStatusResponse>.FromSuccess(result.Value));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CampaignResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCampaignByIdQuery(id), cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<CampaignResponse>.FromSuccess(result.Value));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CampaignResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery(Name = "status")] CampaignStatus[]? statuses = null,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetCampaignsQuery(page, pageSize, statuses), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<PagedResponse<CampaignResponse>>.FromSuccess(result.Value));
    }
}
