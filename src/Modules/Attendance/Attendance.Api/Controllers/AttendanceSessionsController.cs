using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Attendance.Application.Commands;

namespace SchoolErp.Modules.Attendance.Api.Controllers;

[ApiController]
[Route("api/v1/attendance")]
public sealed class AttendanceSessionsController : ControllerBase
{
    private readonly ISender _sender;

    public AttendanceSessionsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateAttendanceSessionCommand(request.TenantId, request.SessionDate), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateAttendanceSessionRequest(Guid TenantId, DateOnly SessionDate);
