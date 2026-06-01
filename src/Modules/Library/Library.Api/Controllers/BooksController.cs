using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Library.Application.Commands;

namespace SchoolErp.Modules.Library.Api.Controllers;

[ApiController]
[Route("api/v1/library")]
public sealed class BooksController : ControllerBase
{
    private readonly ISender _sender;

    public BooksController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateBookCommand(request.TenantId, request.Title, request.Isbn), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateBookRequest(Guid TenantId, string Title, string? Isbn);
