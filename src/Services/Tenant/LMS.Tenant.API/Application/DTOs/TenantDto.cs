namespace LMS.Tenant.API.Application.DTOs;

public sealed record TenantDto(Guid Id, string Name, string Subdomain, string SchemaName, bool IsActive, DateTime CreatedAtUtc);
