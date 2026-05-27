using LMS.Fee.API.Application.DTOs;
using LMS.Fee.API.Domain.Aggregates;
using LMS.Fee.API.Infrastructure.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Fee.API.Application.Queries;

public sealed class ListFeeRecordsQueryHandler : IRequestHandler<ListFeeRecordsQuery, Result<IReadOnlyList<FeeInvoiceDto>>>
{
    private readonly FeeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ListFeeRecordsQueryHandler(FeeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<FeeInvoiceDto>>> Handle(
        ListFeeRecordsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<IReadOnlyList<FeeInvoiceDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var invoices = await _db.FeeInvoices.AsNoTracking().ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLowerInvariant();
            invoices = invoices.Where(i =>
                i.StudentName.ToLowerInvariant().Contains(term) ||
                i.ClassName.ToLowerInvariant().Contains(term)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<FeePaymentStatus>(request.Status, true, out var status))
            invoices = invoices.Where(i => i.Status == status).ToList();

        var dtos = invoices.Select(FeeMappings.ToDto).ToList();
        return Result.Success<IReadOnlyList<FeeInvoiceDto>>(dtos);
    }
}
