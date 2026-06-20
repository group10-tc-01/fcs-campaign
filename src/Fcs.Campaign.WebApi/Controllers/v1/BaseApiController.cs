using System.Diagnostics.CodeAnalysis;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace fcs.Campaign.WebApi.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]

[ExcludeFromCodeCoverage]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected BaseApiController(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected IMediator Mediator { get; }
}
