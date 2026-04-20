using backend.Extensions;
using backend.Extensions.App;
using backend.Extensions.App.Logging;
using backend.Infrastructure.Extensions;
using Serilog;
using System.Runtime.ExceptionServices;

// Configura o Serilog estático para pegar erros antes do Host estar pronto
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando o ecossistema Bueiro Inteligente...");

    var builder = WebApplication.CreateBuilder(args);

    AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
{
    // Ignoramos exceções normais de cancelamento de requisição para não sujar o log
    if (eventArgs.Exception is OperationCanceledException) return;

    // Se o erro envolver Injeção de Dependência, Falha de Instância ou Validação Crítica, ele grita aqui!
    Console.WriteLine($"\n🚨 [ALERTA DE PRIMEIRA CHANCE - ERRO ANTES DO CONTROLLER] 🚨");
    Console.WriteLine($"TIPO: {eventArgs.Exception.GetType().Name}");
    Console.WriteLine($"MENSAGEM: {eventArgs.Exception.Message}");
    Console.WriteLine($"LOCAL: {eventArgs.Exception.TargetSite?.Name}");
    Console.WriteLine($"============================================================\n");
};

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