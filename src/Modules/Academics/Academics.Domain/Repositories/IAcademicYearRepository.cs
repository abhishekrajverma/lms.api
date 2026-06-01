using SchoolErp.Modules.Academics.Domain.Aggregates;

namespace SchoolErp.Modules.Academics.Domain.Repositories;

public interface IAcademicYearRepository
{
    Task AddAsync(AcademicYear entity, CancellationToken cancellationToken = default);
    Task<AcademicYear?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
