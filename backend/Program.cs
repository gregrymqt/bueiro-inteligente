using backend.Extensions;
using backend.Features.Realtime.Filters;
using backend.Infrastructure;
using backend.Infrastructure.Cache;
using Microsoft.AspNetCore.SignalR;
using MonitoringHub = backend.Features.Realtime.Presentation.MonitoringHub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddBueiroInteligenteAuth();
builder.Services.AddBueiroInteligenteSecurity();
builder.Services.AddBueiroInteligenteDatabase();
builder.Services.AddBueiroInteligenteRedis();
builder.Services.AddBueiroInteligenteCache();
builder.Services.AddSingleton<HubExceptionFilter>();
builder.Services.AddSingleton<HubLoggingFilter>();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.AddFilter<HubExceptionFilter>();
    options.AddFilter<HubLoggingFilter>();
});
builder.Services.AddBueiroInteligenteHome();
builder.Services.AddBueiroInteligenteMonitoring();
builder.Services.AddBueiroInteligenteRealtime();
builder.Services.AddBueiroInteligenteScheduler();

var app = builder.Build();

app.Services.InitializeBueiroInteligenteAuth();
app.Services.InitializeBueiroInteligenteSecurity();
await app.Services.InitializeBueiroInteligenteDatabaseAsync();
await app.Services.InitializeBueiroInteligenteRedisAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<MonitoringHub>("/realtime/ws");
app.MapControllers();
app.MapRazorPages();

app.Run();
