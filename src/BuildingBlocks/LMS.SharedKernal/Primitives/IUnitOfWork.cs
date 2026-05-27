namespace LMS.SharedKernal.Primitives;

/// <summary>
/// Unit of Work abstraction.
/// Commit all pending changes and dispatch domain events atomically.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
