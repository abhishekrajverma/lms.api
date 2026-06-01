using MediatR;
using SchoolErp.BuildingBlocks.Application.Cqrs;
using SchoolErp.BuildingBlocks.Domain.Results;
using SchoolErp.Modules.Inventory.Domain.Aggregates;
using SchoolErp.Modules.Inventory.Domain.Repositories;

namespace SchoolErp.Modules.Inventory.Application.Commands;

public sealed record CreateInventoryItemCommand(Guid TenantId, string Sku, string Name) : ICommand<Result<Guid>>;

public sealed class CreateInventoryItemCommandHandler : IRequestHandler<CreateInventoryItemCommand, Result<Guid>>
{
    private readonly IInventoryItemRepository _repository;

    public CreateInventoryItemCommandHandler(IInventoryItemRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var entity = InventoryItem.Create(request.TenantId, request.Sku, request.Name);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
