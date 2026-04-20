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

                // O ASP.NET já loga automaticamente exceções não tratadas.
                // Mas como você prefere logs, você pode injetar um ILogger aqui se desejar um log customizado.

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "Erro interno no servidor",
                        // Em Dev, mostramos a mensagem real da Exception no JSON para facilitar seu debug.
                        // Em Prod, mostramos uma mensagem genérica por segurança.
                        Detail = env.IsDevelopment()
                            ? exception?.Message
                            : "Ocorreu um erro inesperado.",
                        Status = StatusCodes.Status500InternalServerError,
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
}
