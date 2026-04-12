namespace backend.Features.Auth.Domain;

public sealed class User(
    Guid id = default,
    string email = "",
    string hashedPassword = "",
    Guid roleId = default,
    string? fullName = null,
    string? googleId = null,
    string? avatarUrl = null,
    bool emailConfirmed = false
)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string Email { get; set; } = email;

    public string? FullName { get; set; } = fullName;

    public string? GoogleId { get; set; } = googleId;

    public string? AvatarUrl { get; set; } = avatarUrl;

    public bool EmailConfirmed { get; set; } = emailConfirmed;

    public required string HashedPassword { get; set; } = hashedPassword;

    public Guid RoleId { get; set; } = roleId;

    public Role? Role { get; set; }
}

