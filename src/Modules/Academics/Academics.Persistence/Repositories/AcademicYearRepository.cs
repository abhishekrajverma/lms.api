using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Academics.Domain.Aggregates;
using SchoolErp.Modules.Academics.Domain.Repositories;

namespace SchoolErp.Modules.Academics.Persistence.Repositories;

public sealed class AcademicYearRepository : IAcademicYearRepository
{
    private readonly AcademicsDbContext _db;

    public AcademicYearRepository(AcademicsDbContext db) => _db = db;

    public async Task AddAsync(AcademicYear entity, CancellationToken cancellationToken = default)
    {
        await _db.AcademicYears.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<AcademicYear?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.AcademicYears.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
