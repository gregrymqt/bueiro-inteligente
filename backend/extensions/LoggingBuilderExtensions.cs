using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Extensions.App.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddBueiroInteligenteLogging(
        this ILoggingBuilder loggingBuilder,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        // 1. Força a saída no terminal do Docker
        loggingBuilder.AddConsole();
        loggingBuilder.AddDebug();

        // 2. Configura o Log em Arquivo
        string logDirectory = ResolveLogDirectory(configuration, environment);
        var logFileManager = new DailyLogFileManager(logDirectory);

        loggingBuilder.Services.AddSingleton<IDailyLogFileManager>(logFileManager);
        loggingBuilder.AddProvider(new DailyFileLoggerProvider(logFileManager));

        return loggingBuilder;
    }

    private static string ResolveLogDirectory(
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        // Em Docker/Linux, caminhos relativos podem falhar, usamos o ContentRootPath como base segura
        string defaultPath = Path.Combine(environment.ContentRootPath, "Logs");
        string path = configuration["Logging:Directory"] ?? defaultPath;

        return Path.GetFullPath(path);
    }
}

public interface IDailyLogFileManager
{
    string GetCurrentLogFilePath();

    void AppendEntry(string entry);

    Task ClearCurrentAsync(CancellationToken cancellationToken = default);

    Task ClearAsync(DateOnly date, CancellationToken cancellationToken = default);
}

public sealed class DailyLogFileManager : IDailyLogFileManager
{
    private readonly string _logDirectory;
    private readonly object _syncRoot = new();

    public DailyLogFileManager(string logDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logDirectory);

        _logDirectory = Path.GetFullPath(logDirectory);
    }

    public string GetCurrentLogFilePath()
    {
        return GetLogFilePath(DateOnly.FromDateTime(DateTime.Now));
    }

    public void AppendEntry(string entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
        {
            return;
        }

        string filePath = GetCurrentLogFilePath();

        lock (_syncRoot)
        {
            Directory.CreateDirectory(_logDirectory);
            File.AppendAllText(filePath, entry + Environment.NewLine, Encoding.UTF8);
        }
    }

    public Task ClearCurrentAsync(CancellationToken cancellationToken = default)
    {
        return ClearAsync(DateOnly.FromDateTime(DateTime.Now), cancellationToken);
    }

    public Task ClearAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string filePath = GetLogFilePath(date);

        lock (_syncRoot)
        {
            Directory.CreateDirectory(_logDirectory);
            File.WriteAllText(filePath, string.Empty, Encoding.UTF8);
        }

        return Task.CompletedTask;
    }

    private string GetLogFilePath(DateOnly date)
    {
        return Path.Combine(_logDirectory, $"log-{date:dd-MM-yyyy}");
    }
}

internal sealed class DailyFileLoggerProvider : ILoggerProvider
{
    private readonly IDailyLogFileManager _logFileManager;

    public DailyFileLoggerProvider(IDailyLogFileManager logFileManager)
    {
        _logFileManager = logFileManager;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new DailyFileLogger(categoryName, _logFileManager);
    }

    public void Dispose() { }
}

internal sealed class DailyFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IDailyLogFileManager _logFileManager;

    public DailyFileLogger(string categoryName, IDailyLogFileManager logFileManager)
    {
        _categoryName = categoryName;
        _logFileManager = logFileManager;
    }

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        ArgumentNullException.ThrowIfNull(formatter);

        if (!IsEnabled(logLevel))
        {
            return;
        }

        string message = formatter(state, exception) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(message) && exception is null)
        {
            return;
        }

        var builder = new StringBuilder();
        builder.Append(
            DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)
        );
        builder.Append(' ');
        builder.Append('[').Append(logLevel).Append(']');
        builder.Append(' ');
        builder.Append(_categoryName);

        if (eventId.Id != 0 || !string.IsNullOrWhiteSpace(eventId.Name))
        {
            builder.Append(" (");
            builder.Append(eventId.Id);

            if (!string.IsNullOrWhiteSpace(eventId.Name))
            {
                builder.Append(':').Append(eventId.Name);
            }

            builder.Append(')');
        }

        builder.Append(" - ");
        builder.Append(message);

        if (exception is not null)
        {
            builder.AppendLine();
            builder.Append(exception);
        }

        try
        {
            _logFileManager.AppendEntry(builder.ToString());
        }
        catch { }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose() { }
    }
}
