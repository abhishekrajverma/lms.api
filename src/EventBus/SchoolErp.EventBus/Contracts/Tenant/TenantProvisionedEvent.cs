namespace SchoolErp.EventBus.Contracts.Tenant;

public sealed record TenantProvisionedEvent(Guid TenantId, string Code, string Name, DateTime OccurredOnUtc);
