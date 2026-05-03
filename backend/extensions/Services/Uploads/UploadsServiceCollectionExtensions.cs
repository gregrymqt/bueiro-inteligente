using backend.Features.Uploads.Application.Interfaces;
using backend.Features.Uploads.Application.Services;
using backend.Features.Uploads.Domain.Interfaces;
using backend.Features.Uploads.Infrastructure.Repositories;

namespace backend.extensions.Services.Uploads;

public static class UploadsServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteUploads(this IServiceCollection services)
    {
        services.AddScoped<IUploadRepository, UploadRepository>();
        services.AddScoped<IUploadService, UploadService>();

        return services;
    }
}
