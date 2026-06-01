using SchoolErp.Modules.Identity.Domain.Aggregates;

namespace SchoolErp.Modules.Identity.Domain.Repositories;

public interface IUserRepository
{
    Task AddAsync(User entity, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
