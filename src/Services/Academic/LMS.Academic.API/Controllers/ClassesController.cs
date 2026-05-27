using LMS.Academic.API.Application.Commands;
using LMS.Academic.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Academic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ClassesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ClassesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListClassesQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
