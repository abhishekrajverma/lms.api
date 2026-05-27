using LMS.SharedKernal.Results;
using LMS.Tenant.API.Application.DTOs;
using MediatR;

namespace LMS.Tenant.API.Application.Queries;

public sealed record ListTenantsQuery : IRequest<Result<IReadOnlyList<SchoolDto>>>;
