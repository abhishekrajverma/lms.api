using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Employees.Domain.Aggregates;
using SchoolErp.Modules.Employees.Domain.Repositories;

namespace SchoolErp.Modules.Employees.Persistence.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly EmployeesDbContext _db;

    public EmployeeRepository(EmployeesDbContext db) => _db = db;

    public async Task AddAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        await _db.Employees.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Employees.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
