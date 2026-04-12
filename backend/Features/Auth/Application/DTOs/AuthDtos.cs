using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Auth.Application.DTOs;

public sealed record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password
);

public sealed record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType
);

public sealed record TokenPayload(
    string? Sub = null,
    string? Jti = null,
    [property: RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserBase(
    [property: Required, EmailAddress] string Email,
    [property: StringLength(255), JsonPropertyName("full_name")] string? FullName = null,
    [property: RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserCreateRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password,
    [property: StringLength(255), JsonPropertyName("full_name")] string? FullName = null,
    [property: RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserResponse(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("full_name")] string? FullName,
    [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles
);

public sealed record UserInDb(
    [property: Required, EmailAddress] string Email,
    [property: StringLength(255)] string? FullName,
    [property: Required] string HashedPassword,
    [property: RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserTokenData(string Email, string Role, string Jti);