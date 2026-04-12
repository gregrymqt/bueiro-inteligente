namespace backend.Features.Auth.Domain;

public sealed class Role(Guid id = default, string name = "", string? description = null)
{
    public Guid Id { get; set; } = id == Guid.Empty ? Guid.NewGuid() : id;

    public required string Name { get; set; } = name;

    public string? Description { get; set; } = description;

    public ICollection<User> Users { get; set; } = [];
}