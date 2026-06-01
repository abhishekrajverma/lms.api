using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolErp.Modules.Inventory.Application.Commands;

namespace SchoolErp.Modules.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
public sealed class InventoryItemsController : ControllerBase
{
    private readonly ISender _sender;

    public InventoryItemsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateInventoryItemCommand(request.TenantId, request.Sku, request.Name), cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);
        return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
    }
}

public sealed record CreateInventoryItemRequest(Guid TenantId, string Sku, string Name);
