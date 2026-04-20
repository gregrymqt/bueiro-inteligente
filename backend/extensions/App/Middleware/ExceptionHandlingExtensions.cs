using backend.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace backend.Extensions.App.Middleware;

public static class ExceptionHandlingExtensions
{
    public static void UseBueiroInteligenteExceptionHandling(
        this IApplicationBuilder app,
        IWebHostEnvironment env
    )
    {
        app.UseExceptionHandler(handler =>
        {
            handler.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("GlobalExceptionHandler");

                var (statusCode, title, detail) = ResolveProblemDetails(exception, env);

                try
                {
                    logger.LogError(
                        exception,
                        "ERRO CRÍTICO: Rota {Method} {Path} falhou com status {StatusCode}. Mensagem: {Message}",
                        context.Request.Method,
                        context.Request.Path,
                        statusCode,
                        exception?.Message
                    );
                }
                catch
                {
                    Log.ForContext("SourceContext", "GlobalExceptionHandler").Error(
                        exception,
                        "ERRO CRÍTICO: Rota {Method} {Path} falhou com status {StatusCode}. Mensagem: {Message}",
                        context.Request.Method,
                        context.Request.Path,
                        statusCode,
                        exception?.Message
                    );
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = title,
                        Detail = detail,
                        Status = statusCode,
                        Instance = context.Request.Path,
                    }
                );
            });
        });

        if (!env.IsDevelopment())
        {
            app.UseHsts();
        }

        // Mantemos seu middleware de status codes (404, 403) para rotas /api
        app.Use(
            async (context, next) =>
            {
                await next();

                if (
                    context.Response.StatusCode >= 400
                    && context.Request.Path.StartsWithSegments("/api")
                    && !context.Response.HasStarted
                )
                {
                    context.Response.ContentType = "application/problem+json";
                }
            }
        );
    }

    private static (int StatusCode, string Title, string Detail) ResolveProblemDetails(
    Exception? exception,
    IWebHostEnvironment env
)
    {
        // TÁTICA KAMIKAZE: Ignora se é Development, ignora segurança.
        // Vamos forçar o erro real (com Stack Trace completa) a ir direto para o Frontend!

        var errorDetail = exception != null
            ? exception.ToString() // Pega a mensagem e a linha exata onde explodiu
            : "MISTÉRIO FATAL: A exceção chegou NULA no handler!";

        return (
            500,
            "FANTASMA CAPTURADO",
            errorDetail
        );
    }
}
