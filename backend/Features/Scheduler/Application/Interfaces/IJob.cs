namespace backend.Features.Scheduler.Application.Interfaces;

public interface IJob<in TResource>
{
    Task ExecuteAsync(TResource resource);
}