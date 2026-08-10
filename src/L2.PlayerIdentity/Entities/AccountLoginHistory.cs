namespace L2.PlayerIdentity.Entities;

public sealed class AccountLoginHistory
{
    public Guid Id { get; set; }
    public Guid? AccountId { get; set; }
    public required string NormalizedEmail { get; set; }
    public bool Succeeded { get; set; }
    public string? FailureCode { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Account? Account { get; set; }
}
