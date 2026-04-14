using backend.Core;
using backend.Extensions.Auth.Abstractions;
using backend.Extensions.Auth.Infrastructure;
using backend.Extensions.Auth.Models;
using backend.Features.Auth.Application.Services;
using backend.Features.Auth.Infrastructure.Authentication;
using backend.Features.Auth.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace backend.Extensions.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteAuth(this IServiceCollection services)
    {
        AppSettings settings = AppSettings.Current;
        GoogleSettings googleSettings = new(
            settings.GoogleClientId,
            settings.GoogleClientSecret,
            GoogleRedirectUrlResolver.ResolvePreferredFrontendRedirectUrl(settings.AllowedOrigins),
            settings.AllowedOrigins
        );

        services.AddSingleton(settings);
        services.AddSingleton(googleSettings);
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
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = ".BueiroInteligente.External";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddScheme<AuthenticationSchemeOptions, BueiroInteligenteAuthenticationHandler>(
                BueiroInteligenteAuthenticationDefaults.Scheme,
                _ => { }
            )
            .AddGoogle(options =>
            {
                options.ClientId = googleSettings.ClientId;
                options.ClientSecret = googleSettings.ClientSecret;
                options.CallbackPath = GoogleAuthDefaults.CallbackPath;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey("picture", "picture");
            });
        services.AddAuthorization();
        return services;
    }

}