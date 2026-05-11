using backend.Features.Drains.Domain.Interfaces;
using backend.Features.Drains.Infrastructure.Persistence.Repositories;
using backend.Features.Feedbacks.Domain.Interfaces;
using backend.Features.Feedbacks.Infrastructure.Persistence.Repositories;
using backend.Features.Home.Domain.Interfaces;
using backend.Features.Home.Infrastructure.Persistence.Repositories;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Features.Monitoring.Infrastructure.Persistence.Repositories;
using backend.Features.Notifications.Domain.Interfaces;
using backend.Features.Notifications.Infrastructure.Persistence.Repositories;
using backend.Features.Payment.Domain.Interfaces;
using backend.Features.Payment.infrastructure.Persistence.Repositories;
using backend.Features.Subscription.Domain.Interfaces;
using backend.Features.Subscription.Infrastructure.Persistence.Repositories;
using backend.Infrastructure.Persistence; // Necessário para referenciar o AppDbContext
using Scrutor;

namespace backend.Infrastructure.Extensions;

public static class DependencyScanningExtensions
{
    public static IServiceCollection AddBueiroInteligenteDependencyScanning(
        this IServiceCollection services
    )
    {
        services.Scan(scan =>
            scan
            // Usamos FromAssembliesOf para garantir que ele escaneie o projeto principal[cite: 36]
            .FromAssembliesOf(typeof(AppDbContext))
                .AddClasses(classes =>
                    classes.Where(type =>
                        IsRepositoryOrService(type) && !type.Name.StartsWith("Cached")
                    )
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        // Register Decorators here to wrap the real repositories
        services.Decorate<IHomeRepository, CachedHomeRepository>();
        services.Decorate<IFeedbackRepository, CachedFeedbackRepository>();
        services.Decorate<IDrainRepository, CachedDrainRepository>();
        services.Decorate<IMonitoringRepository, CachedMonitoringRepository>();
        services.Decorate<INotificationRepository, CachedNotificationRepository>();
        services.Decorate<IPaymentRepository, CachedPaymentRepository>();
        services.Decorate<ISubscriptionRepository, CachedSubscriptionRepository>();

        return services;
    }

    private static bool IsRepositoryOrService(Type type) =>
        type.Name.EndsWith("Repository") || type.Name.EndsWith("Service");
}
