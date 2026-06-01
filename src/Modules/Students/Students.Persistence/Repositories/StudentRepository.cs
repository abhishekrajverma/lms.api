using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Students.Domain.Aggregates;
using SchoolErp.Modules.Students.Domain.Repositories;

namespace SchoolErp.Modules.Students.Persistence.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly StudentsDbContext _db;

    public StudentRepository(StudentsDbContext db) => _db = db;

    public async Task AddAsync(Student entity, CancellationToken cancellationToken = default)
    {
        await _db.Students.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Students.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
