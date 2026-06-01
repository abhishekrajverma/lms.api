using Hangfire;

namespace SchoolErp.Hangfire.Jobs;

public static class RetentionJobs
{
    public static void Register() =>
        RecurringJob.AddOrUpdate("audit-retention", () => RunRetentionAsync(), Cron.Weekly);

    public static Task RunRetentionAsync() => Task.CompletedTask;
}
