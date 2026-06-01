using SchoolErp.Modules.Students.Domain.Aggregates;

namespace SchoolErp.Modules.Students.Domain.Repositories;

public interface IStudentRepository
{
    Task AddAsync(Student entity, CancellationToken cancellationToken = default);
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
