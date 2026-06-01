using SchoolErp.Modules.Examinations.Domain.Aggregates;

namespace SchoolErp.Modules.Examinations.Domain.Repositories;

public interface IExamRepository
{
    Task AddAsync(Exam entity, CancellationToken cancellationToken = default);
    Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
