using backend.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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

                // Use o logger injetado para capturar o erro real
                var logger = context.RequestServices.GetRequiredService<ILogger<AppIdMiddleware>>();

                var (statusCode, title, detail) = ResolveProblemDetails(exception, env);

                // LOG CRÍTICO: Isso vai aparecer com o Stack Trace completo no arquivo .txt
                logger.LogError(exception, "ERRO CRÍTICO: Rota {Method} {Path} falhou com status {StatusCode}. Mensagem: {Message}",
                    context.Request.Method, context.Request.Path, statusCode, exception?.Message);

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
        return exception switch
        {
            LogicException or ArgumentException => (400, "Validation error", exception.Message),
            NotFoundException => (404, "Not found", exception.Message),
            UnauthorizedAccessException => (401, "Unauthorized", exception.Message),
            ConnectionException => (503, "Connection error", exception.Message),
            ExternalApiException => (502, "External API error", exception.Message),
            null => (
                500,
                "Erro interno no servidor",
                "Erro interno. Verifique os logs do servidor."
            ),
            _ => env.IsDevelopment()
                ? (500, "Erro interno no servidor", exception.Message)
                : (
                    500,
                    "Erro interno no servidor",
                    "Erro interno. Verifique os logs do servidor."
                ),
        };
    }
}
