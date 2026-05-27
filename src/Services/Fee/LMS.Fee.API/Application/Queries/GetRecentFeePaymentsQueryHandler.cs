using LMS.Fee.API.Application.DTOs;
using LMS.Fee.API.Infrastructure.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Fee.API.Application.Queries;

public sealed class GetRecentFeePaymentsQueryHandler
    : IRequestHandler<GetRecentFeePaymentsQuery, Result<IReadOnlyList<RecentFeePaymentDto>>>
{
    private readonly FeeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetRecentFeePaymentsQueryHandler(FeeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<RecentFeePaymentDto>>> Handle(
        GetRecentFeePaymentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<IReadOnlyList<RecentFeePaymentDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var limit = Math.Clamp(request.Limit, 1, 50);
        var invoices = await _db.FeeInvoices
            .AsNoTracking()
            .OrderByDescending(i => i.PaidAtUtc ?? i.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var dtos = invoices.Select(FeeMappings.ToRecentPayment).ToList();
        return Result.Success<IReadOnlyList<RecentFeePaymentDto>>(dtos);
    }
}
