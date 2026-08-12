using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("game_sessions")]
public sealed class GameSession
{
    [Key, Column("id")]
    public Guid Id { get; set; }
    [Column("account_session_id")]
    public Guid AccountSessionId { get; set; }
    [Column("game_version"), MaxLength(32)]
    public required string GameVersion { get; set; }
    [Column("access_token_hash")]
    public byte[] AccessTokenHash { get; set; } = [];
    [Column("selected_character_id")]
    public Guid? SelectedCharacterId { get; set; }
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [Column("last_seen_at")]
    public DateTimeOffset LastSeenAt { get; set; }
    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
    [Column("revoked_at")]
    public DateTimeOffset? RevokedAt { get; set; }
    public AccountSession AccountSession { get; set; } = null!;
    public GameVersion Version { get; set; } = null!;
}
