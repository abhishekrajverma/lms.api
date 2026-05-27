using LMS.Fee.API.Application.DTOs;
using LMS.Fee.API.Domain.Aggregates;
using LMS.Fee.API.Infrastructure.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Fee.API.Application.Queries;

public sealed class GetFeeSummaryQueryHandler : IRequestHandler<GetFeeSummaryQuery, Result<FeeSummaryDto>>
{
    private readonly FeeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetFeeSummaryQueryHandler(FeeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<FeeSummaryDto>> Handle(GetFeeSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<FeeSummaryDto>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var invoices = await _db.FeeInvoices.AsNoTracking().ToListAsync(cancellationToken);
        var collected = invoices.Sum(i => i.PaidAmount);
        var pending = invoices.Sum(i => i.PendingAmount);
        var overdue = invoices.Where(i => i.Status == FeePaymentStatus.Overdue).Sum(i => i.PendingAmount);

        return Result.Success(new FeeSummaryDto(collected, pending, overdue));
    }
}
