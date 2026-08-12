namespace L2.Server.Services.Interfaces;

public sealed record GameSessionState(
    Guid SessionId,
    Guid AccountId,
    string Username,
    string GameVersion,
    Guid? SelectedCharacterId,
    DateTimeOffset ExpiresAt);
