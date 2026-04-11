using BueiroInteligente.Extensions;
using BueiroInteligente.Infrastructure;
using BueiroInteligente.Infrastructure.Cache;
using MonitoringHub = BueiroInteligente.Features.Realtime.Presentation.MonitoringHub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddBueiroInteligenteAuth();
builder.Services.AddBueiroInteligenteSecurity();
builder.Services.AddBueiroInteligenteDatabase();
builder.Services.AddBueiroInteligenteRedis();
builder.Services.AddBueiroInteligenteCache();
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
