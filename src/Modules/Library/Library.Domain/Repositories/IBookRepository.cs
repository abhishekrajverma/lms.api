using SchoolErp.Modules.Library.Domain.Aggregates;

namespace SchoolErp.Modules.Library.Domain.Repositories;

public interface IBookRepository
{
    Task AddAsync(Book entity, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
