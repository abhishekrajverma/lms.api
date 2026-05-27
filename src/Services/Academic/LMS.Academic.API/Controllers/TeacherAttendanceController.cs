using LMS.Academic.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Academic.API.Controllers;

[ApiController]
[Route("api/attendance/teachers")]
public sealed class TeacherAttendanceController : ControllerBase
{
    private readonly IMediator _mediator;
    public TeacherAttendanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet("today")]
    public async Task<IActionResult> Today(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeacherAttendanceTodayQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
