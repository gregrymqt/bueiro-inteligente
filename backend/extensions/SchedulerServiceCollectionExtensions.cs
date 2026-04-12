using backend.Features.Rows.Application.Jobs;
using backend.Features.Rows.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace backend.Extensions;

/// <summary>
/// Registers Quartz.NET and schedules the Rows synchronization job.
/// </summary>
public static class SchedulerServiceCollectionExtensions
{
    private const string JobIdentity = "rows-sync-job";
    private const string TriggerIdentity = "rows-sync-trigger";

    public static IServiceCollection AddBueiroInteligenteScheduler(
        this IServiceCollection services
    )
    {
        services.AddBueiroInteligenteRows();
        services.AddTransient<RowsSyncJob>();

        services.AddQuartz(quartz =>
        {
            quartz.AddJob<RowsSyncJob>(options => options.WithIdentity(JobIdentity));

            quartz.AddTrigger(options =>
                options
                    .ForJob(JobIdentity)
                    .WithIdentity(TriggerIdentity)
                    .StartAt(DateBuilder.FutureDate(60, IntervalUnit.Minute))
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInMinutes(60).RepeatForever()
                    )
            );
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }
}