using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("game_session_tickets")]
public sealed class GameSessionTicket
{
    [Key, Column("id")]
    public Guid Id { get; set; }
    [Column("account_session_id")]
    public Guid AccountSessionId { get; set; }
    [Column("game_version"), MaxLength(32)]
    public required string GameVersion { get; set; }
    [Column("token_hash")]
    public required byte[] TokenHash { get; set; }
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
    [Column("consumed_at")]
    public DateTimeOffset? ConsumedAt { get; set; }
    public AccountSession AccountSession { get; set; } = null!;
    public GameVersion Version { get; set; } = null!;
}
