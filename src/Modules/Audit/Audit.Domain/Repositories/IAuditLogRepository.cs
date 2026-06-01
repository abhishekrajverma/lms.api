using SchoolErp.Modules.Audit.Domain.Aggregates;

namespace SchoolErp.Modules.Audit.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entity, CancellationToken cancellationToken = default);
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
