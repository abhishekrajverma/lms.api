using LMS.Academic.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Academic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExamsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ExamsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("upcoming")]
    public async Task<IActionResult> Upcoming([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetUpcomingExamsQuery(limit), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
