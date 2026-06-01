using SchoolErp.BuildingBlocks.Domain;

namespace SchoolErp.Modules.Library.Domain.Aggregates;

public sealed class Book : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Isbn { get; private set; }

    private Book(Guid id, Guid tenantId, string title, string? isbn)
    {
        Id = id;
        TenantId = tenantId;
        Title = title;
        Isbn = isbn;
    }

    public static Book Create(Guid tenantId, string title, string? isbn) =>
        new(Guid.NewGuid(), tenantId, title, isbn);
}
