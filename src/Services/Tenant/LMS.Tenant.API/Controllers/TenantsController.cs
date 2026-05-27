using LMS.Tenant.API.Application.Commands;
using LMS.Tenant.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Tenant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterTenantCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapError(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListTenantsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTenantByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapError(result);
    }

    private IActionResult MapError(LMS.SharedKernal.Results.Result result) =>
        result.Error.Code switch
        {
            "Tenant.NotFound" => NotFound(result.Error),
            "Tenant.SubdomainExists" => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
}
