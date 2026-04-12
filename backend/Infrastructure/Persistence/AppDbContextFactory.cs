using backend.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace backend.Infrastructure.Persistence;

// Esta classe é lida APENAS pelas ferramentas 'dotnet ef'
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Usa a MIGRATIONS_URL (Porta 5432) definida no seu .env
        string connectionString = AppSettings.Current.MigrationsUrl;

        // Se a string vier no formato postgres://, converte para o formato .NET
        string resolvedConnectionString =
            DatabaseServiceCollectionExtensions.PostgreSqlConnectionStringFactory.Create(
                connectionString
            );

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(resolvedConnectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
