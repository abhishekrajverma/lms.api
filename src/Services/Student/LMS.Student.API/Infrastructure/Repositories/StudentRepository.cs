using LMS.Student.API.Domain.Aggregates;
using LMS.Student.API.Domain.Repositories;
using LMS.Student.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMS.Student.API.Infrastructure.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _db;

    public StudentRepository(StudentDbContext db) => _db = db;

    public Task<StudentProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(StudentProfile entity, CancellationToken cancellationToken = default) =>
        await _db.Students.AddAsync(entity, cancellationToken);

    public void Update(StudentProfile entity) => _db.Students.Update(entity);

    public void Remove(StudentProfile entity) => _db.Students.Remove(entity);
}
