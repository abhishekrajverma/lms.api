using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Transport.Application.Commands;

namespace SchoolErp.Modules.Transport.Api.Controllers;

[ApiController]
[Route("api/v1/transport")]
public sealed class RoutesController : ControllerBase
{
    private readonly ISender _sender;

    public RoutesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRouteRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateRouteCommand(request.TenantId, request.Name), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateRouteRequest(Guid TenantId, string Name);
