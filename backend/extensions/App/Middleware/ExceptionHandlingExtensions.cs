using backend.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

                var (statusCode, title, detail) = ResolveProblemDetails(context, exception, env);

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
        HttpContext context,
        Exception? exception,
        IWebHostEnvironment env
    )
    {
        var logger = context
            .RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");

        var frontendDetail = env.IsDevelopment()
            ? exception?.Message ?? "MISTÉRIO FATAL: A exceção chegou NULA no handler!"
            : "Ocorreu um erro interno ao processar a requisição.";

        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}.",
            context.Request.Method,
            context.Request.Path
        );

        return (500, "oi", frontendDetail);
    }
}
