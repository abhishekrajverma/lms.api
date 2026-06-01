using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Library.Domain.Aggregates;
using SchoolErp.Modules.Library.Domain.Repositories;

namespace SchoolErp.Modules.Library.Persistence.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _db;

    public BookRepository(LibraryDbContext db) => _db = db;

    public async Task AddAsync(Book entity, CancellationToken cancellationToken = default)
    {
        await _db.Books.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
