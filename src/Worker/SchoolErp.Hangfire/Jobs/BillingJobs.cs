using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SchoolErp.Hangfire.Jobs;

public static class BillingJobs
{
    public static void Register()
    {
        RecurringJob.AddOrUpdate("billing-renewal", () => RunRenewalAsync(), Cron.Daily);
    }

    public static Task RunRenewalAsync() => Task.CompletedTask;
}

public sealed class HangfireJobScheduler : IHostedService
{
    private readonly ILogger<HangfireJobScheduler> _logger;

    public HangfireJobScheduler(ILogger<HangfireJobScheduler> logger) => _logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        BillingJobs.Register();
        RetentionJobs.Register();
        _logger.LogInformation("Hangfire recurring jobs registered");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
