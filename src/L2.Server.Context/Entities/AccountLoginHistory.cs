using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("account_login_history")]
public sealed class AccountLoginHistory
{
    [Key, Column("id")]
    public Guid Id { get; set; }
    [Column("account_id")]
    public Guid? AccountId { get; set; }
    [Column("game_version"), MaxLength(32)]
    public required string GameVersion { get; set; }
    [Column("normalized_email"), MaxLength(254)]
    public required string NormalizedEmail { get; set; }
    [Column("succeeded")]
    public bool Succeeded { get; set; }
    [Column("failure_code"), MaxLength(40)]
    public string? FailureCode { get; set; }
    [Column("ip_address"), MaxLength(64)]
    public string? IpAddress { get; set; }
    [Column("user_agent"), MaxLength(512)]
    public string? UserAgent { get; set; }
    [Column("occurred_at")]
    public DateTimeOffset OccurredAt { get; set; }
    public Account? Account { get; set; }
    public GameVersion Version { get; set; } = null!;
}
