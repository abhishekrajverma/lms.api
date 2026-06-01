using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Identity.Domain.Aggregates;
using SchoolErp.Modules.Identity.Domain.Repositories;

namespace SchoolErp.Modules.Identity.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _db;

    public UserRepository(IdentityDbContext db) => _db = db;

    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await _db.Users.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
