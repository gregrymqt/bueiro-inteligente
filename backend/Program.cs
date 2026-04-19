using System.Text.Json;
using backend.Extensions;
using backend.Extensions.App.Logging;
using backend.Extensions.App.Middleware;
using backend.Extensions.Auth;
using backend.Extensions.Realtime;
using backend.Extensions.Security;
using backend.Features.Realtime.Filters;
using backend.Infrastructure;
using backend.Infrastructure.Cache;
using backend.Infrastructure.Extensions;
using Microsoft.AspNetCore.SignalR;
using MonitoringHub = backend.Features.Realtime.Presentation.MonitoringHub;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddBueiroInteligenteDotEnvMappings();
builder.Services.AddBueiroInteligenteOptions(builder.Configuration);
builder.Configuration.AddEnvironmentVariables();

builder.Logging.AddBueiroInteligenteFileLogging(builder.Configuration, builder.Environment);

// Add services to the container.
builder.Services.AddRazorPages();
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    });
builder.Services.AddBueiroInteligenteSecurity();
builder.Services.AddBueiroInteligenteDatabase();
builder.Services.AddBueiroInteligenteRedis();
builder.Services.AddBueiroInteligenteCache();
builder.Services.AddSingleton<HubExceptionFilter>();
builder.Services.AddSingleton<HubLoggingFilter>();
builder
    .Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = true;
        options.AddFilter<HubExceptionFilter>();
        options.AddFilter<HubLoggingFilter>();
    })
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.PayloadSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    });
builder.Services.AddBueiroInteligenteHome();
builder.Services.AddBueiroInteligenteMonitoring();
builder.Services.AddBueiroInteligenteRealtime();
builder.Services.AddBueiroInteligenteScheduler();
builder.Services.AddBueiroInteligenteAuth(builder.Configuration);
builder.Services.AddBueiroInteligenteApp();

var app = builder.Build();

await app.Services.GetRequiredService<AuthExtension>().OpenAsync();
app.Services.InitializeBueiroInteligenteSecurity();
await app.Services.InitializeBueiroInteligenteDatabaseAsync();
await app.Services.InitializeBueiroInteligenteRedisAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors(AppServiceCollectionExtensions.RestrictedOriginsPolicyName);

app.UseBueiroInteligenteApp();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<MonitoringHub>("/realtime/ws");
app.MapControllers();
app.MapRazorPages();
app.MapGet("/health", () => Results.Ok()).AllowAnonymous();

app.Run();
