using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Fees.Application.Commands;

namespace SchoolErp.Modules.Fees.Api.Controllers;

[ApiController]
[Route("api/v1/fees")]
public sealed class FeeInvoicesController : ControllerBase
{
    private readonly ISender _sender;

    public FeeInvoicesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateFeeInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateFeeInvoiceCommand(request.TenantId, request.InvoiceNumber), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateFeeInvoiceRequest(Guid TenantId, string InvoiceNumber);
