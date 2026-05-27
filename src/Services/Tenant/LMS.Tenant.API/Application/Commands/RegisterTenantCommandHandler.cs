using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using LMS.Tenant.API.Application.DTOs;
using LMS.Tenant.API.Domain.Aggregates;
using LMS.Tenant.API.Domain.Repositories;
using MediatR;

namespace LMS.Tenant.API.Application.Commands;

public sealed class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, Result<TenantDto>>
{
    private readonly ISchoolTenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterTenantCommandHandler(ISchoolTenantRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TenantDto>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetBySubdomainAsync(request.Subdomain, cancellationToken);
        if (existing is not null)
            return Result.Failure<TenantDto>(Error.Conflict("Tenant.SubdomainExists", "Subdomain is already registered."));

        var tenant = SchoolTenant.Register(request.Name, request.Subdomain);
        await _repository.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Subdomain,
            tenant.SchemaName,
            tenant.IsActive,
            tenant.CreatedAtUtc));
    }
}
