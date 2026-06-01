using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Billing.Domain.Aggregates;
using SchoolErp.Modules.Billing.Domain.Repositories;

namespace SchoolErp.Modules.Billing.Application.Commands;

public sealed record CreateSubscriptionCommand(Guid TenantId, string PlanCode) : ICommand<Result<Guid>>;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<Guid>>
{
    private readonly ISubscriptionRepository _repository;

    public CreateSubscriptionCommandHandler(ISubscriptionRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var entity = Subscription.Create(request.TenantId, request.PlanCode);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
