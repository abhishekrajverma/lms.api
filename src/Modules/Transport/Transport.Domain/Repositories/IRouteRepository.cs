using SchoolErp.Modules.Transport.Domain.Aggregates;

namespace SchoolErp.Modules.Transport.Domain.Repositories;

public interface IRouteRepository
{
    Task AddAsync(Route entity, CancellationToken cancellationToken = default);
    Task<Route?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
