namespace L2.PlayerIdentity.Entities;

public sealed class AccountCredential
{
    public Guid AccountId { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Account Account { get; set; } = null!;
}
