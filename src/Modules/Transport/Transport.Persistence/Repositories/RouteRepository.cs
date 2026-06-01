using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Transport.Domain.Aggregates;
using SchoolErp.Modules.Transport.Domain.Repositories;

namespace SchoolErp.Modules.Transport.Persistence.Repositories;

public sealed class RouteRepository : IRouteRepository
{
    private readonly TransportDbContext _db;

    public RouteRepository(TransportDbContext db) => _db = db;

    public async Task AddAsync(Route entity, CancellationToken cancellationToken = default)
    {
        await _db.Routes.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Routes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
