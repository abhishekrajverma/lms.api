using LMS.Tenant.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Tenant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SchoolsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SchoolsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListTenantsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
