namespace backend.Features.Scheduler.Application.Interfaces;

public interface IQueueService
{
    Task EnqueueJobAsync<TJob, TResource>(TResource resource)
        where TJob : IJob<TResource>;
}