namespace L2.PlayerIdentity.Entities;

public sealed class AccountSession
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public required byte[] TokenHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Account Account { get; set; } = null!;
    public ICollection<GameSessionTicket> GameTickets { get; set; } = [];
}
