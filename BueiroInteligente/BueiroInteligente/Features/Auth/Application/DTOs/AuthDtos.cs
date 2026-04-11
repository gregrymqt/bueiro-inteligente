using System.ComponentModel.DataAnnotations;

namespace BueiroInteligente.Features.Auth.Application.DTOs;

public sealed record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password
);

public sealed record TokenResponse(string AccessToken, string TokenType);

public sealed record TokenPayload(
    string? Sub = null,
    string? Jti = null,
    [property: RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserBase(
    [property: Required, EmailAddress] string Email,
    [property: StringLength(255)] string? FullName = null,
    [property: RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserCreateRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password,
    [property: StringLength(255)] string? FullName = null,
    [property: RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserResponse(string Email, string? FullName, string Role);

public sealed record UserInDb(
    [property: Required, EmailAddress] string Email,
    [property: StringLength(255)] string? FullName,
    [property: Required] string HashedPassword,
    [property: RegularExpression("^(Admin|Manager|User)$", ErrorMessage = "Role must be Admin, Manager or User.")] string Role = "User"
);

public sealed record UserTokenData(string Email, string Role, string Jti);