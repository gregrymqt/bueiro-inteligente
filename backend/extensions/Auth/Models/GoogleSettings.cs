namespace backend.Extensions.Auth.Models;

public sealed record GoogleSettings(
    string ClientId,
    string ClientSecret,
    string FrontendRedirectUrl,
    string[] AllowedOrigins
);
