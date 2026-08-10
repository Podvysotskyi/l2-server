using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("accounts")]
public sealed class Account
{
    [Key, Column("id")]
    public Guid Id { get; set; }
    [Column("username"), MaxLength(24)]
    public required string Username { get; set; }
    [Column("normalized_username"), MaxLength(24)]
    public required string NormalizedUsername { get; set; }
    [Column("email"), MaxLength(254)]
    public required string Email { get; set; }
    [Column("normalized_email"), MaxLength(254)]
    public required string NormalizedEmail { get; set; }
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    public AccountCredential? Credential { get; set; }
    public ICollection<AccountSession> Sessions { get; set; } = [];
    public ICollection<AccountLoginHistory> LoginHistory { get; set; } = [];
}
