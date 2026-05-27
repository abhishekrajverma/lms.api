using LMS.Student.API.Application.Commands;
using LMS.Student.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Student.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? @class,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new ListStudentsQuery(search, @class, status, page, pageSize),
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudentsSummaryQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudentByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value) : BadRequest(result.Error);
    }
}
