using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("account_sessions")]
public sealed class AccountSession
{
    [Key, Column("id")]
    public Guid Id { get; set; }
    [Column("account_id")]
    public Guid AccountId { get; set; }
    [Column("token_hash")]
    public required byte[] TokenHash { get; set; }
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [Column("last_seen_at")]
    public DateTimeOffset LastSeenAt { get; set; }
    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
    [Column("revoked_at")]
    public DateTimeOffset? RevokedAt { get; set; }
    public Account Account { get; set; } = null!;
    public ICollection<GameSessionTicket> GameTickets { get; set; } = [];
    public ICollection<GameSession> GameSessions { get; set; } = [];
}
