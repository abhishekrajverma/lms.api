using LMS.Student.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Student.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAttendanceSummaryQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("students")]
    public async Task<IActionResult> Students(
        [FromQuery] string? @class,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudentAttendanceRowsQuery(@class, search), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("heatmap")]
    public async Task<IActionResult> Heatmap([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAttendanceHeatmapQuery(months), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> Weekly(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWeeklyAttendanceQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
