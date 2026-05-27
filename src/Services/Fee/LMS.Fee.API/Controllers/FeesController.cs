using LMS.Fee.API.Application.Commands;
using LMS.Fee.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Fee.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FeesController : ControllerBase
{
    private readonly IMediator _mediator;
    public FeesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("records")]
    public async Task<IActionResult> Records(
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListFeeRecordsQuery(status, search), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeeSummaryQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("collection/monthly")]
    public async Task<IActionResult> MonthlyCollection([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMonthlyFeeCollectionQuery(months), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("payments/recent")]
    public async Task<IActionResult> RecentPayments([FromQuery] int limit = 5, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetRecentFeePaymentsQuery(limit), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeeInvoiceCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Created(string.Empty, result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{invoiceId:guid}/pay")]
    public async Task<IActionResult> Pay(Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RecordFeePaymentCommand(invoiceId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
