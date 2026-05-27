using LMS.SharedKernal.Guards;
using LMS.SharedKernal.Persistence;

namespace LMS.Notification.API.Domain.Aggregates;

public enum NoticePriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

public sealed class NotificationMessage : TenantAggregateRoot
{
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsSent { get; private set; }
    public NoticePriority Priority { get; private set; } = NoticePriority.Medium;
    public DateOnly PublishedDate { get; private set; }

    private NotificationMessage(Guid id, Guid tenantId) : base(id, tenantId) { }
    private NotificationMessage() { }

    public static NotificationMessage Create(
        Guid tenantId,
        string subject,
        string body,
        NoticePriority priority = NoticePriority.Medium,
        DateOnly? publishedDate = null)
    {
        Guard.AgainstEmptyGuid(tenantId);
        Guard.AgainstNullOrEmpty(subject);
        Guard.AgainstNullOrEmpty(body);
        return new NotificationMessage(Guid.NewGuid(), tenantId)
        {
            Subject = subject.Trim(),
            Body = body.Trim(),
            Priority = priority,
            PublishedDate = publishedDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }

    public void MarkSent() => IsSent = true;
}
