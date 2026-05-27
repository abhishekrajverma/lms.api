using LMS.SharedKernal.Results;
using LMS.Tenant.API.Application.DTOs;
using MediatR;

namespace LMS.Tenant.API.Application.Commands;

public sealed record RegisterTenantCommand(string Name, string Subdomain) : IRequest<Result<TenantDto>>;
