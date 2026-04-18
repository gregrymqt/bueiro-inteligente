using backend.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace backend.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = AppSettings.Current.MigrationsUrl;
        var resolvedString =
            DatabaseServiceCollectionExtensions.PostgreSqlConnectionStringFactory.Create(
                connectionString
            );

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(resolvedString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
