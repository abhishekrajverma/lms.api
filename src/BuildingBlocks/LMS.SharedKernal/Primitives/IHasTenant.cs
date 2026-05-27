namespace LMS.SharedKernal.Primitives;

/// <summary>
/// Marker interface for all tenant-scoped entities.
/// Entities implementing this will be automatically filtered by TenantId
/// in TenantAwareDbContext and have TenantId stamped on write.
/// </summary>
public interface IHasTenant
{
    Guid TenantId { get; }
}
