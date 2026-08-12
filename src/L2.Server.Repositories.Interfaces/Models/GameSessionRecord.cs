namespace L2.Server.Repositories.Interfaces;

public sealed record GameSessionRecord(
    Guid SessionId,
    Guid AccountId,
    string Username,
    string GameVersion,
    string GameServer,
    Guid? SelectedCharacterId,
    DateTimeOffset ExpiresAt);
