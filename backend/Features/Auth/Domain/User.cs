namespace backend.Features.Auth.Domain;

public sealed class User(
    Guid id = default,
    string email = "",
    string hashedPassword = "",
    Guid roleId = default,
    string? fullName = null
)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string Email { get; set; } = email;

    public string? FullName { get; set; } = fullName;

    public required string HashedPassword { get; set; } = hashedPassword;

    public Guid RoleId { get; set; } = roleId;

    public Role? Role { get; set; }
}