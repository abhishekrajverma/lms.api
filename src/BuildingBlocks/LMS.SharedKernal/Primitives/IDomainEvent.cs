using MediatR;

namespace LMS.SharedKernal.Primitives;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
