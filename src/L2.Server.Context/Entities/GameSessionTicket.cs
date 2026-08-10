namespace L2.Server.Context.Entities;

public sealed class GameSessionTicket
{
    public Guid Id { get; set; }
    public Guid AccountSessionId { get; set; }
    public required byte[] TokenHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public AccountSession AccountSession { get; set; } = null!;
}
