using LMS.EventBus.Outbox;
using LMS.Student.API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LMS.Student.API.Infrastructure.Outbox;

public sealed class StudentOutboxProcessor : OutboxProcessorBase<StudentDbContext>
{
    public StudentOutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<StudentOutboxProcessor> logger)
        : base(scopeFactory, logger) { }
}
