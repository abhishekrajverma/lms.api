using LMS.Academic.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Academic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TransportController : ControllerBase
{
    private readonly IMediator _mediator;
    public TransportController(IMediator mediator) => _mediator = mediator;

    [HttpGet("routes")]
    public async Task<IActionResult> Routes([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListTransportRoutesQuery(status), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransportSummaryQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
