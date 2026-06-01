namespace SchoolErp.BuildingBlocks.Domain;

public abstract class AuditableEntity<TId> : Entity<TId> where TId : notnull
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public string? CreatedBy { get; protected set; }
    public DateTime? ModifiedAtUtc { get; protected set; }
    public string? ModifiedBy { get; protected set; }
}
