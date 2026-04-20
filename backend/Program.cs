using backend.Extensions;
using backend.Extensions.App;
using backend.Extensions.App.Logging;
using backend.Infrastructure.Extensions;
using Serilog;

// Configura o Serilog estático para pegar erros antes do Host estar pronto
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando o ecossistema Bueiro Inteligente...");

    var builder = WebApplication.CreateBuilder(args);

    // 1. Configurações e Logs
    builder.Configuration.AddEnvironmentVariables().AddBueiroInteligenteDotEnvMappings();
    builder.Host.AddBueiroInteligenteLogging(builder.Configuration, builder.Environment);

    builder.Services.AddBueiroInteligenteServices(
        builder.Configuration,
        builder.Environment
    ); // Agrupador de serviços

    var app = builder.Build();

    // 3. Executa o Pipeline e as Inicializações
    await app.UseBueiroInteligentePipeline();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar: {Message}", ex.Message);
}
finally
{
    Log.CloseAndFlush();
}