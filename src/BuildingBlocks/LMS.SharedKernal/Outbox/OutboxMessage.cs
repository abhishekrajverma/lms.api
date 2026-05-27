namespace LMS.SharedKernal.Outbox;

/// <summary>
/// Represents a domain event serialized and persisted in the same DB transaction.
/// A background OutboxProcessor reads these and publishes to RabbitMQ,
/// guaranteeing at-least-once delivery without distributed transactions.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    /// <summary>Full CLR type name of the domain event.</summary>
    public string Type { get; private set; }

    /// <summary>JSON-serialized domain event payload.</summary>
    public string Payload { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string type, string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            OccurredOnUtc = DateTime.UtcNow
        };
    }

    public void MarkProcessed() => ProcessedOnUtc = DateTime.UtcNow;

    public void MarkFailed(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        Error = error;
    }
}
