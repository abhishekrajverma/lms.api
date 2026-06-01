using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Academics.Application.Commands;

namespace SchoolErp.Modules.Academics.Api.Controllers;

[ApiController]
[Route("api/v1/academics")]
public sealed class AcademicYearsController : ControllerBase
{
    private readonly ISender _sender;

    public AcademicYearsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAcademicYearRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateAcademicYearCommand(request.TenantId, request.Name, request.StartDate, request.EndDate), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateAcademicYearRequest(Guid TenantId, string Name, DateOnly StartDate, DateOnly EndDate);
