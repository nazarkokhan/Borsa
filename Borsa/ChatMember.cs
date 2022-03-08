namespace Borsa;

public record ChatMember(
    int Id,
    string? FirstName,
    string? LastName,
    string? AvatarUrl,
    string Role);