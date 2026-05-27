using LMS.SharedKernal.Results;
using LMS.Tenant.API.Application.DTOs;
using LMS.Tenant.API.Domain.Repositories;
using MediatR;

namespace LMS.Tenant.API.Application.Queries;

public sealed class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
{
    private readonly ISchoolTenantRepository _repository;

    public GetTenantByIdQueryHandler(ISchoolTenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (tenant is null)
            return Result.Failure<TenantDto>(Error.NotFound("Tenant.NotFound", "Tenant was not found."));

        return Result.Success(new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Subdomain,
            tenant.SchemaName,
            tenant.IsActive,
            tenant.CreatedAtUtc));
    }
}
