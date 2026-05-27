using LMS.Student.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Student.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AdmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdmissionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("recent")]
    public async Task<IActionResult> Recent([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetRecentAdmissionsQuery(limit), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
