using LMS.Academic.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Academic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PayrollController : ControllerBase
{
    private readonly IMediator _mediator;
    public PayrollController(IMediator mediator) => _mediator = mediator;

    [HttpGet("records")]
    public async Task<IActionResult> Records(
        [FromQuery] string? status,
        [FromQuery] string? department,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListPayrollRecordsQuery(status, department), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPayrollSummaryQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("approvals/pending")]
    public async Task<IActionResult> PendingApprovals(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingSalaryApprovalsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
