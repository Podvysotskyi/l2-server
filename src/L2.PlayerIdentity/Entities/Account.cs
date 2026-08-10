namespace L2.PlayerIdentity.Entities;

public sealed class Account
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string NormalizedUsername { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public AccountCredential? Credential { get; set; }
    public ICollection<AccountSession> Sessions { get; set; } = [];
    public ICollection<AccountLoginHistory> LoginHistory { get; set; } = [];
}
