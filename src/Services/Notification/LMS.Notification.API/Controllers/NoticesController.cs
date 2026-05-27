using LMS.Notification.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class NoticesController : ControllerBase
{
    private readonly IMediator _mediator;
    public NoticesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListNoticesQuery(limit), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
