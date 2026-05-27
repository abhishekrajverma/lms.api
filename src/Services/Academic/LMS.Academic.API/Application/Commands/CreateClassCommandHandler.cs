using LMS.Academic.API.Application.DTOs;
using LMS.Academic.API.Domain.Aggregates;
using LMS.Academic.API.Domain.Repositories;
using LMS.SharedKernal.Primitives;
using LMS.SharedKernal.Results;
using MediatR;

namespace LMS.Academic.API.Application.Commands;

public sealed class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, Result<ClassDto>>
{
    private readonly IClassRepository _repository;
    private readonly ITenantContext _tenant;
    private readonly IUnitOfWork _uow;

    public CreateClassCommandHandler(IClassRepository repository, ITenantContext tenant, IUnitOfWork uow)
    {
        _repository = repository;
        _tenant = tenant;
        _uow = uow;
    }

    public async Task<Result<ClassDto>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        if (!_tenant.IsResolved)
            return Result.Failure<ClassDto>(Error.Unauthorized("Tenant.Required", "Tenant context is required."));

        var schoolClass = SchoolClass.Create(_tenant.TenantId, request.Name, request.Grade);
        await _repository.AddAsync(schoolClass, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success(new ClassDto(schoolClass.Id, schoolClass.Name, schoolClass.Grade));
    }
}
