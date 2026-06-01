using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Communication.Domain.Aggregates;
using SchoolErp.Modules.Communication.Domain.Repositories;

namespace SchoolErp.Modules.Communication.Application.Commands;

public sealed record CreateNotificationCommand(Guid TenantId, string Subject) : ICommand<Result<Guid>>;

public sealed class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    private readonly INotificationRepository _repository;

    public CreateNotificationCommandHandler(INotificationRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var entity = Notification.Create(request.TenantId, request.Subject);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
