using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Students.Application.Commands;

namespace SchoolErp.Modules.Students.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateStudentCommand(request.TenantId, request.FirstName, request.LastName), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateStudentRequest(Guid TenantId, string FirstName, string LastName);
