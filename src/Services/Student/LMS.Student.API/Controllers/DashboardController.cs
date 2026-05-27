using LMS.Student.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Student.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("admissions")]
    public async Task<IActionResult> Admissions([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAdmissionTrendQuery(months), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
