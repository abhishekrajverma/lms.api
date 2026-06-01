using SchoolErp.Modules.Employees.Domain.Aggregates;

namespace SchoolErp.Modules.Employees.Domain.Repositories;

public interface IEmployeeRepository
{
    Task AddAsync(Employee entity, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
