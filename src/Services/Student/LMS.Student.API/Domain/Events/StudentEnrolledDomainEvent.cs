using LMS.SharedKernal.Primitives;

namespace LMS.Student.API.Domain.Events;

public sealed record StudentEnrolledDomainEvent(Guid StudentId, Guid TenantId, string AdmissionNumber) : DomainEventBase;
