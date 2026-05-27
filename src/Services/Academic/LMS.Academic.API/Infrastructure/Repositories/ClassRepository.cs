using LMS.Academic.API.Domain.Aggregates;
using LMS.Academic.API.Domain.Repositories;
using LMS.Academic.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMS.Academic.API.Infrastructure.Repositories;

public sealed class ClassRepository : IClassRepository
{
    private readonly AcademicDbContext _db;
    public ClassRepository(AcademicDbContext db) => _db = db;
    public Task<SchoolClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Classes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task AddAsync(SchoolClass entity, CancellationToken cancellationToken = default) =>
        await _db.Classes.AddAsync(entity, cancellationToken);
    public void Update(SchoolClass entity) => _db.Classes.Update(entity);
    public void Remove(SchoolClass entity) => _db.Classes.Remove(entity);
}
