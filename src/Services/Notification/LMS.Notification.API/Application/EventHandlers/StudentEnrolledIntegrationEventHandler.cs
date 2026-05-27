using LMS.EventBus.Abstractions;
using LMS.Notification.API.Domain.Aggregates;
using LMS.Notification.API.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace LMS.Notification.API.Application.EventHandlers;

public sealed class StudentEnrolledIntegrationEvent : IntegrationEvent
{
    public Guid StudentId { get; }
    public Guid TenantId { get; }
    public string AdmissionNumber { get; }

    public StudentEnrolledIntegrationEvent(Guid studentId, Guid tenantId, string admissionNumber)
    {
        StudentId = studentId;
        TenantId = tenantId;
        AdmissionNumber = admissionNumber;
    }
}

public sealed class StudentEnrolledIntegrationEventHandler
    : IIntegrationEventHandler<StudentEnrolledIntegrationEvent>
{
    private readonly NotificationDbContext _db;
    private readonly ILogger<StudentEnrolledIntegrationEventHandler> _logger;

    public StudentEnrolledIntegrationEventHandler(
        NotificationDbContext db,
        ILogger<StudentEnrolledIntegrationEventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(StudentEnrolledIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending welcome notification for student {StudentId}", @event.StudentId);

        var message = NotificationMessage.Create(
            @event.TenantId,
            "Welcome",
            $"Student {@event.AdmissionNumber} enrolled successfully.");

        await _db.Set<NotificationMessage>().AddAsync(message, cancellationToken);
        message.MarkSent();
        await _db.SaveChangesAsync(cancellationToken);
    }
}
