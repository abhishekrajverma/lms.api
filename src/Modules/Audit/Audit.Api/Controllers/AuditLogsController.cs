using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Audit.Application.Commands;

namespace SchoolErp.Modules.Audit.Api.Controllers;

[ApiController]
[Route("api/v1/audit")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly ISender _sender;

    public AuditLogsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateAuditLogCommand(request.TenantId, request.Action), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateAuditLogRequest(Guid TenantId, string Action);
