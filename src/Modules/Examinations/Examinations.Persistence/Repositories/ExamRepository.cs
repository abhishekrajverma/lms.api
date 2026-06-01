using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Examinations.Domain.Aggregates;
using SchoolErp.Modules.Examinations.Domain.Repositories;

namespace SchoolErp.Modules.Examinations.Persistence.Repositories;

public sealed class ExamRepository : IExamRepository
{
    private readonly ExaminationsDbContext _db;

    public ExamRepository(ExaminationsDbContext db) => _db = db;

    public async Task AddAsync(Exam entity, CancellationToken cancellationToken = default)
    {
        await _db.Exams.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Exams.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
