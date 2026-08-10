using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace L2.Server.Context.Entities;

[Table("account_credentials")]
public sealed class AccountCredential
{
    [Key, Column("account_id")]
    public Guid AccountId { get; set; }
    [Column("password_hash")]
    public required string PasswordHash { get; set; }
    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
    public Account Account { get; set; } = null!;
}
