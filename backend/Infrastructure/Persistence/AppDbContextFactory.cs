using backend.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace backend.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddBueiroInteligenteDotEnvMappings()
                .Build();

        var connectionString =
            configuration.GetConnectionString("MigrationsConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection strings 'MigrationsConnection' ou 'DefaultConnection' não definidas."
            );
        var resolvedString =
            DatabaseServiceCollectionExtensions.PostgreSqlConnectionStringFactory.Create(
                connectionString
            );

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(resolvedString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
