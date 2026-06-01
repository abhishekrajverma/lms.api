using Microsoft.EntityFrameworkCore;
using SchoolErp.Modules.Inventory.Domain.Aggregates;
using SchoolErp.Modules.Inventory.Domain.Repositories;

namespace SchoolErp.Modules.Inventory.Persistence.Repositories;

public sealed class InventoryItemRepository : IInventoryItemRepository
{
    private readonly InventoryDbContext _db;

    public InventoryItemRepository(InventoryDbContext db) => _db = db;

    public async Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default)
    {
        await _db.InventoryItems.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.InventoryItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
