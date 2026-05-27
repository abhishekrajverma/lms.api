using LMS.Notification.API.Application.DTOs;
using LMS.Notification.API.Infrastructure.Persistence;
using LMS.SharedKernal.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _db;
    private readonly ITenantContext _tenant;

    public NotificationsController(NotificationDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!_tenant.IsResolved)
            return BadRequest(new { code = "Tenant.Required", message = "Tenant context is required." });

        var items = await _db.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.PublishedDate)
            .Take(10)
            .Select(n => new NotificationItemDto(
                n.Subject,
                n.Body,
                n.PublishedDate.ToString("yyyy-MM-dd")))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
}
