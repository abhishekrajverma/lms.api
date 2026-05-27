using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Student.API.Application.DTOs;
using LMS.Student.API.Domain.Aggregates;
using LMS.Student.API.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.API.Application.Queries;

public sealed class ListStudentsQueryHandler : IRequestHandler<ListStudentsQuery, Result<PagedStudentsDto>>
{
    private readonly StudentDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ListStudentsQueryHandler(StudentDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedStudentsDto>> Handle(ListStudentsQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<PagedStudentsDto>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var query = _db.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(term) ||
                s.LastName.ToLower().Contains(term) ||
                s.AdmissionNumber.ToLower().Contains(term) ||
                s.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Class))
            query = query.Where(s => s.ClassName == request.Class.Trim());

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<StudentStatus>(request.Status, true, out var status))
            query = query.Where(s => s.Status == status);

        var total = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);

        var items = await query
            .OrderBy(s => s.ClassName)
            .ThenBy(s => s.AdmissionNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(StudentMappings.ToListItem).ToList();
        return Result.Success(new PagedStudentsDto(dtos, total));
    }
}
