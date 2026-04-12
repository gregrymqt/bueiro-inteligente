using backend.Features.Auth.Domain;
using backend.Features.Home.Domain;
using backend.Features.Monitoring.Domain;
using backend.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Persistence;

using DrainEntity = backend.Features.Drain.Domain.Drain;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    public DbSet<DrainEntity> Drains => Set<DrainEntity>();

    public DbSet<CarouselModel> HomeCarousels => Set<CarouselModel>();

    public DbSet<StatCardModel> HomeStats => Set<StatCardModel>();

    public DbSet<DrainStatus> DrainStatuses => Set<DrainStatus>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
