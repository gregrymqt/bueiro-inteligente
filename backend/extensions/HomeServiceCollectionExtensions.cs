using backend.Features.Home.Application.Services;
using backend.Features.Home.Application.Interfaces;
using backend.Features.Home.Domain.Interfaces;
using backend.Features.Home.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions;

/// <summary>
/// Registers the Home vertical slice services.
/// </summary>
public static class HomeServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteHome(this IServiceCollection services)
    {
        services.AddScoped<IHomeRepository, HomeRepository>();
        services.AddScoped<IHomeService, HomeService>();

        return services;
    }
}