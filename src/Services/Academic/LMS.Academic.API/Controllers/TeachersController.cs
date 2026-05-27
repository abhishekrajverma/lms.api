using LMS.Academic.API.Application.Commands;
using LMS.Academic.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Academic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;
    public TeachersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListTeachersQuery(search, department, status), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeachersSummaryQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeacherCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
