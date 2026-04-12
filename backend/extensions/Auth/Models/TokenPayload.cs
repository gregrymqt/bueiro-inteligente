using System.Collections.Generic;

namespace backend.Extensions.Auth.Models;

public sealed record TokenPayload(
    string Email,
    string Role = "User",
    IReadOnlyDictionary<string, object?>? AdditionalClaims = null
);
