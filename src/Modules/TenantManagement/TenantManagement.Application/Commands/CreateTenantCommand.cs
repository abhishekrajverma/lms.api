using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.TenantManagement.Domain.Aggregates;
using SchoolErp.Modules.TenantManagement.Domain.Repositories;

namespace SchoolErp.Modules.TenantManagement.Application.Commands;

public sealed record CreateTenantCommand(string Name, string Code) : ICommand<Result<Guid>>;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    private readonly ITenantRepository _repository;

    public CreateTenantCommandHandler(ITenantRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var entity = Tenant.Create(request.Name, request.Code);
        entity.Provision();
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
