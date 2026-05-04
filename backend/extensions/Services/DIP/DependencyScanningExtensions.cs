using Scrutor;
using backend.Infrastructure.Persistence; // Necessário para referenciar o AppDbContext

namespace backend.Infrastructure.Extensions;

public static class DependencyScanningExtensions
{
    public static IServiceCollection AddBueiroInteligenteDependencyScanning(this IServiceCollection services)
    {
        services.Scan(scan => scan
            // Usamos FromAssembliesOf para garantir que ele escaneie o projeto principal[cite: 36]
            .FromAssembliesOf(typeof(AppDbContext)) 
            .AddClasses(classes => classes
                .Where(type =>
                    type.Name.EndsWith("Repository") ||
                    type.Name.EndsWith("Service")
                ))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}