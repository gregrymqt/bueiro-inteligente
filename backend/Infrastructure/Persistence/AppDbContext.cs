using backend.Features.Auth.Domain;
using backend.Features.Drains.Domain;
using backend.Features.Home.Domain;
using backend.Features.Monitoring.Domain;
using backend.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Drain> Drains => Set<Drain>();
    public DbSet<CarouselModel> HomeCarousels => Set<CarouselModel>();
    public DbSet<StatCardModel> HomeStats => Set<StatCardModel>();
    public DbSet<DrainStatus> DrainStatuses => Set<DrainStatus>();
    public DbSet<backend.Features.Uploads.Domain.UploadModel> Uploads => Set<backend.Features.Uploads.Domain.UploadModel>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
