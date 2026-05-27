namespace LMS.Tenant.API.Application.DTOs;

public sealed record SchoolDto(Guid Id, string Name, string Code, string Plan, string City);
