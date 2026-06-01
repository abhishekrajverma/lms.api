using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Audit.Domain.Aggregates;
using SchoolErp.Modules.Audit.Domain.Repositories;

namespace SchoolErp.Modules.Audit.Application.Commands;

public sealed record CreateAuditLogCommand(Guid TenantId, string Action) : ICommand<Result<Guid>>;

public sealed class CreateAuditLogCommandHandler : IRequestHandler<CreateAuditLogCommand, Result<Guid>>
{
    private readonly IAuditLogRepository _repository;

    public CreateAuditLogCommandHandler(IAuditLogRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateAuditLogCommand request, CancellationToken cancellationToken)
    {
        var entity = AuditLog.Create(request.TenantId, request.Action);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
