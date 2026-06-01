using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Inventory.Domain.Aggregates;

public sealed class InventoryItem : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private InventoryItem(Guid id, Guid tenantId, string sku, string name)
    {
        Id = id;
        TenantId = tenantId;
        Sku = sku;
        Name = name;
    }

    public static InventoryItem Create(Guid tenantId, string sku, string name) =>
        new(Guid.NewGuid(), tenantId, sku, name);
}
