using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Examinations.Application.Commands;

namespace SchoolErp.Modules.Examinations.Api.Controllers;

[ApiController]
[Route("api/v1/examinations")]
public sealed class ExamsController : ControllerBase
{
    private readonly ISender _sender;

    public ExamsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateExamRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateExamCommand(request.TenantId, request.Title), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateExamRequest(Guid TenantId, string Title);
