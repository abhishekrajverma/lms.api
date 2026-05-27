using LMS.Academic.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Academic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TimetableController : ControllerBase
{
    private readonly IMediator _mediator;
    public TimetableController(IMediator mediator) => _mediator = mediator;

    [HttpGet("today")]
    public async Task<IActionResult> Today(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTodaysClassesQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
