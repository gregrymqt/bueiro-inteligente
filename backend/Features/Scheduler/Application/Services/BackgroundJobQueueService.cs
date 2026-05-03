using backend.Features.Scheduler.Application.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

// using backend.Core.Exceptions; // Descomente caso tenha suas exceções globais

namespace backend.Features.Scheduler.Application.Services;

public class BackgroundJobQueueService(
    IBackgroundJobClient backgroundJobClient,
    ILogger<BackgroundJobQueueService> logger
) : IQueueService
{
    public Task EnqueueJobAsync<TJob, TResource>(TResource resource)
        where TJob : IJob<TResource>
    {
        if (resource == null)
        {
            throw new ArgumentNullException(
                nameof(resource),
                "O recurso (payload) não pode ser nulo."
            );
        }

        try
        {
            var jobName = typeof(TJob).Name;

            // Note o uso do @ em {@Payload} para que o Serilog serialize o objeto no log estruturado
            logger.LogInformation(
                "Enfileirando job do tipo '{JobName}' com o payload: {@Payload}",
                jobName,
                resource
            );

            // Enfileira o job de forma genérica
            backgroundJobClient.Enqueue<TJob>(job => job.ExecuteAsync(resource));

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Falha ao enfileirar o job do tipo {JobName}. O job NÃO foi agendado.",
                typeof(TJob).Name
            );

            // Substitua 'Exception' pela sua AppServiceException para manter o padrão de erros do seu sistema
            throw new Exception("Falha ao agendar a tarefa de processamento.", ex);
        }
    }
}
