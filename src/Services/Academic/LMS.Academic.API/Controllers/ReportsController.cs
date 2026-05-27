using LMS.Academic.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Academic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("revenue-growth")]
    public async Task<IActionResult> RevenueGrowth([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetRevenueGrowthQuery(months), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMonthlyRevenueReportQuery(months), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> Expenses(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetExpenseCategoriesQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("academics/classes")]
    public async Task<IActionResult> ClassPerformance(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClassPerformanceQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("academics/subjects")]
    public async Task<IActionResult> SubjectPerformance(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubjectPerformanceQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("attendance/trend")]
    public async Task<IActionResult> AttendanceTrend(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAttendanceTrendQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("salary-distribution")]
    public async Task<IActionResult> SalaryDistribution(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSalaryDistributionQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
