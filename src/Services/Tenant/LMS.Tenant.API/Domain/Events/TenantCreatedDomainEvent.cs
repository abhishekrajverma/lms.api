using LMS.SharedKernal.Primitives;

namespace LMS.Tenant.API.Domain.Events;

public sealed record TenantCreatedDomainEvent(Guid TenantId, string Name, string Subdomain) : DomainEventBase;
