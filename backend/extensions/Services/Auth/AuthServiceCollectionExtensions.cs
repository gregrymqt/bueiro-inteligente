using System.Security.Claims;
using backend.Core.Settings;
using backend.extensions.Services.Auth.Abstractions;
using backend.extensions.Services.Auth.Infrastructure;
using backend.Features.Auth.Application.Interfaces;
using backend.Features.Auth.Application.Services;
using backend.Features.Auth.Domain.Interfaces;
using backend.Features.Auth.Infrastructure.Authentication;
using backend.Features.Auth.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace backend.extensions.Services.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddBueiroInteligenteAuth(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var googleSettings =
            configuration.GetSection(GoogleSettings.SectionName).Get<GoogleSettings>()
            ?? new GoogleSettings();

        services.Configure<GoogleSettings>(configuration.GetSection(GoogleSettings.SectionName));
        services.AddSingleton<ITokenBlacklistStore, InMemoryTokenBlacklistStore>();
        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
        services.AddSingleton<AuthExtension>();
        services.AddSingleton<IAuthExtension>(sp => sp.GetRequiredService<AuthExtension>());
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = BueiroInteligenteAuthenticationDefaults.Scheme;
                options.DefaultChallengeScheme = BueiroInteligenteAuthenticationDefaults.Scheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(
                IdentityConstants.ExternalScheme,
                options =>
                {
                    options.Cookie.Name = ".BueiroInteligente.External";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            )
            .AddScheme<AuthenticationSchemeOptions, BueiroInteligenteAuthenticationHandler>(
                BueiroInteligenteAuthenticationDefaults.Scheme,
                _ => { }
            );

        if (
            !string.IsNullOrWhiteSpace(googleSettings.GoogleClientId)
            && !string.IsNullOrWhiteSpace(googleSettings.GoogleClientSecret)
        )
        {
            services
                .AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = googleSettings.GoogleClientId;
                    options.ClientSecret = googleSettings.GoogleClientSecret;
                    options.CallbackPath = GoogleAuthDefaults.CallbackPath;
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey("picture", "picture");
                });
        }

        services.AddAuthorization();
        return services;
    }
}
