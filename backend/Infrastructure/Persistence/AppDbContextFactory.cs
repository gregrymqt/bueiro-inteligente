using backend.Core.Settings;
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

        var settings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>()
            ?? new DatabaseSettings();
        var connectionString = settings.MigrationsUrl;
        var resolvedString =
            DatabaseServiceCollectionExtensions.PostgreSqlConnectionStringFactory.Create(
                connectionString
            );

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(resolvedString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
