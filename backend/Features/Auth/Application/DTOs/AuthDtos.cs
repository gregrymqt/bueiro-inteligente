using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Features.Auth.Application.DTOs;

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public sealed record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType
);

public sealed record TokenPayload(
    string? Sub = null,
    string? Jti = null,
    [RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserBase(
    [Required, EmailAddress] string Email,
    [StringLength(255)] [property: JsonPropertyName("full_name")] string? FullName = null,
    [RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserCreateRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [StringLength(255)] [property: JsonPropertyName("full_name")] string? FullName = null,
    [RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserResponse(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("full_name")] string? FullName,
    [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles
);

public sealed record UserInDb(
    [Required, EmailAddress] string Email,
    [StringLength(255)] string? FullName,
    [Required] string HashedPassword,
    [RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserTokenData(string Email, string Role, string Jti);