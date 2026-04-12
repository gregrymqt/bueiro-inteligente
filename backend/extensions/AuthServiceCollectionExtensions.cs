using backend.Core;
using backend.Extensions.Auth.Abstractions;
using backend.Extensions.Auth.Infrastructure;
using backend.Features.Auth.Application.Services;
using backend.Features.Auth.Infrastructure.Authentication;
using backend.Features.Auth.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Extensions.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteAuth(this IServiceCollection services)
    {
        services.AddSingleton(AppSettings.Current);
        services.AddSingleton<ITokenBlacklistStore, InMemoryTokenBlacklistStore>();
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
        services.AddSingleton<AuthExtension>();
        services.AddSingleton<IAuthExtension>(sp => sp.GetRequiredService<AuthExtension>());
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = BueiroInteligenteAuthenticationDefaults.Scheme;
                options.DefaultChallengeScheme = BueiroInteligenteAuthenticationDefaults.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, BueiroInteligenteAuthenticationHandler>(
                BueiroInteligenteAuthenticationDefaults.Scheme,
                _ => { }
            );
        services.AddAuthorization();
        return services;
    }
}
