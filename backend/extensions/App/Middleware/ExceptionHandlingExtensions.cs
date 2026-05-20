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

    // Remova a dependência desnecessária do ILoggerFactory
    private static (int StatusCode, string Title, string Detail) ResolveProblemDetails(
    HttpContext context,
    Exception? exception,
    IWebHostEnvironment env
)
    {
        // Log usando o motor que já provou funcionar na inicialização
        Serilog.Log.Error(
            exception,
            "Unhandled exception while processing {Method} {Path}.",
            context.Request.Method,
            context.Request.Path
        );

        // Mapeamento dinâmico de Exceções de Domínio para Status Code HTTP
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            LogicException => (StatusCodes.Status400BadRequest, "Regra de negócio violada"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro Interno")
        };

        var frontendDetail = env.IsDevelopment()
            ? exception?.Message ?? "MISTÉRIO FATAL: A exceção chegou NULA no handler!"
            : statusCode == 500
                ? "Ocorreu um erro interno ao processar a requisição."
                : exception?.Message; // Para erros 400/404, é seguro e útil enviar a mensagem de domínio para o front

        return (statusCode, title, frontendDetail!);
    }
}