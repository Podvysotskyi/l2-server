namespace L2.Server.Services.Interfaces;

public sealed record GameSessionState(
    Guid SessionId,
    Guid AccountId,
    string Username,
    Guid? SelectedCharacterId,
    DateTimeOffset ExpiresAt);
