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
        if (env.IsDevelopment())
        {
            // Em dev, mostra a página detalhada padrão para facilitar o seu debug
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // Em produção/Render, captura exceções e retorna JSON
            app.UseExceptionHandler(handler =>
            {
                handler.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";

                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionFeature?.Error;

                    await context.Response.WriteAsJsonAsync(
                        new ProblemDetails
                        {
                            Title = "Erro interno no servidor",
                            Detail = exception?.Message ?? "Ocorreu um erro inesperado.",
                            Status = StatusCodes.Status500InternalServerError,
                            Instance = context.Request.Path,
                        }
                    );
                });
            });

            app.UseHsts();
        }

        // Adicione este bloco no final do seu método UseBueiroInteligenteExceptionHandling
        app.Use(
            async (context, next) =>
            {
                await next();

                // Se chegou aqui com 404, significa que nem o Controller nem o SignalR acharam a rota
                if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(
                        new ProblemDetails
                        {
                            Title = "Não Encontrado",
                            Detail = $"A rota '{context.Request.Path}' não existe nesta API.",
                            Status = 404,
                        }
                    );
                }
            }
        );
    }
}
