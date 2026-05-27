using LMS.Fee.API.Application.DTOs;
using LMS.Fee.API.Infrastructure.Persistence;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LMS.Fee.API.Application.Queries;

public sealed class GetMonthlyFeeCollectionQueryHandler
    : IRequestHandler<GetMonthlyFeeCollectionQuery, Result<IReadOnlyList<MonthlyFeeCollectionDto>>>
{
    private readonly FeeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public GetMonthlyFeeCollectionQueryHandler(FeeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<MonthlyFeeCollectionDto>>> Handle(
        GetMonthlyFeeCollectionQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<IReadOnlyList<MonthlyFeeCollectionDto>>(
                Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var from = DateTime.UtcNow.AddMonths(-request.Months);
        var invoices = await _db.FeeInvoices
            .AsNoTracking()
            .Where(i => i.CreatedAtUtc >= from)
            .ToListAsync(cancellationToken);

        var grouped = invoices
            .GroupBy(i => new { i.CreatedAtUtc.Year, i.CreatedAtUtc.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyFeeCollectionDto(
                new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM", CultureInfo.InvariantCulture),
                g.Sum(i => i.PaidAmount),
                g.Sum(i => i.PendingAmount)))
            .ToList();

        return Result.Success<IReadOnlyList<MonthlyFeeCollectionDto>>(grouped);
    }
}
