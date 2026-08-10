namespace L2.Server.Context.Entities;

public sealed class GameSession
{
    public Guid Id { get; set; }
    public Guid AccountSessionId { get; set; }
    public byte[] AccessTokenHash { get; set; } = [];
    public Guid? SelectedCharacterId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public AccountSession AccountSession { get; set; } = null!;
}
