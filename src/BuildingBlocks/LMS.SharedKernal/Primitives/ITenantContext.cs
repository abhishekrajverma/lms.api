namespace LMS.SharedKernal.Primitives;

/// <summary>
/// Provides the current tenant identity, resolved from JWT claim or request header.
/// Register as Scoped — one instance per HTTP request.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantIdentifier { get; }
    bool IsResolved { get; }
}
