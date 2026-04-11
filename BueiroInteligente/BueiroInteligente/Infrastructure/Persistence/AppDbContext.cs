using BueiroInteligente.Features.Auth.Domain;
using BueiroInteligente.Features.Home.Domain;
using BueiroInteligente.Features.Monitoring.Domain;
using BueiroInteligente.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace BueiroInteligente.Infrastructure.Persistence;

using DrainEntity = BueiroInteligente.Features.Drain.Domain.Drain;

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
