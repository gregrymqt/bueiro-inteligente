namespace backend.Features.Auth.Infrastructure.Authentication;

public static class GoogleAuthDefaults
{
    public const string CallbackPath = "/api/v1/auth/google-signin";

    public const string RedirectPath = "/api/v1/auth/google-callback";

    public const string AccessTokenCookieName = "access_token";
}

