using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Audit.Domain.Aggregates;
using SchoolErp.Modules.Audit.Domain.Repositories;

namespace SchoolErp.Modules.Audit.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AuditDbContext _db;

    public AuditLogRepository(AuditDbContext db) => _db = db;

    public async Task AddAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        await _db.AuditLogs.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.AuditLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
