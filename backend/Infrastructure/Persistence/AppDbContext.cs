using backend.Features.Auth.Domain;
using backend.Features.Auth.Domain.Entities;
using backend.Features.Drains.Domain;
using backend.Features.Drains.Domain.Entities;
using backend.Features.Feedbacks.Domain.Entities;
using backend.Features.Home.Domain;
using backend.Features.Home.Domain.Entities;
using backend.Features.Monitoring.Domain;
using backend.Features.Monitoring.Domain.Entities;
using backend.Features.Notifications.Domain.Entities;
using backend.Features.Payment.Domain;
using backend.Features.Payment.Domain.Entities;
using backend.Features.Subscription.Domain.Entities;
using backend.Features.Uploads.Domain;
using backend.Features.Uploads.Domain.Entities;
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

    public DbSet<UploadModel> Uploads =>
        Set<UploadModel>();

    public DbSet<PaymentTransaction> PaymentTransactions =>
        Set<PaymentTransaction>();

    public DbSet<UserSubscription> UserSubscriptions =>
        Set<UserSubscription>();

    public DbSet<SubscriptionPlan> SubscriptionPlans =>
        Set<SubscriptionPlan>();

    public DbSet<Notification> Notifications =>
        Set<Notification>();

    public DbSet<Feedback> Feedbacks =>
        Set<Feedback>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}