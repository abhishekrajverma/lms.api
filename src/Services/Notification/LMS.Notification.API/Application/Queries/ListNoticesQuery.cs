using LMS.Notification.API.Application.DTOs;
using LMS.Notification.API.Infrastructure.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Notification.API.Application.Queries;

public sealed record ListNoticesQuery(int Limit = 20) : IRequest<Result<IReadOnlyList<SchoolNoticeDto>>>;

public sealed class ListNoticesQueryHandler : IRequestHandler<ListNoticesQuery, Result<IReadOnlyList<SchoolNoticeDto>>>
{
    private readonly NotificationDbContext _db;
    private readonly ITenantContext _tenant;

    public ListNoticesQueryHandler(NotificationDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<Result<IReadOnlyList<SchoolNoticeDto>>> Handle(
        ListNoticesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenant.IsResolved)
            return Result.Failure<IReadOnlyList<SchoolNoticeDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var limit = Math.Clamp(request.Limit, 1, 100);
        var items = await _db.Notifications
            .AsNoTracking()
            .OrderByDescending(n => n.PublishedDate)
            .Take(limit)
            .Select(n => new SchoolNoticeDto(
                n.Id,
                n.Subject,
                n.PublishedDate.ToString("yyyy-MM-dd"),
                n.Priority.ToString().ToLowerInvariant()))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SchoolNoticeDto>>(items);
    }
}
