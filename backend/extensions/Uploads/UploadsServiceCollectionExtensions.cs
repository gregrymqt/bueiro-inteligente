using backend.Features.Uploads.Application.Services;
using backend.Features.Uploads.Domain.Interfaces;
using backend.Features.Uploads.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.Uploads;

public static class UploadsServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteUploads(this IServiceCollection services)
    {
        services.AddScoped<IUploadRepository, UploadRepository>();
        services.AddScoped<IUploadService, UploadService>();

        return services;
    }
}
