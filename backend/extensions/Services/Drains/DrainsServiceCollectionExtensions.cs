using backend.Features.Drains.Application.Services;
using backend.Features.Drains.Domain.Interfaces;
using backend.Features.Drains.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions;

public static class DrainsServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteDrains(this IServiceCollection services)
    {
        services.AddScoped<IDrainRepository, DrainRepository>();
        services.AddScoped<IDrainService, DrainService>();

        return services;
    }
}