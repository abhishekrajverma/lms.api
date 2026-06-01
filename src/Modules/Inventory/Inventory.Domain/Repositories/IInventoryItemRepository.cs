using SchoolErp.Modules.Inventory.Domain.Aggregates;

namespace SchoolErp.Modules.Inventory.Domain.Repositories;

public interface IInventoryItemRepository
{
    Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
