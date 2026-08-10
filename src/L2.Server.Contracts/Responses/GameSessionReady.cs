namespace L2.Server.Contracts;

public sealed record GameSessionReady(
    string Type,
    Guid AccountId,
    string Username,
    PlayerCharacterSummary Character,
    int ProtocolVersion,
    string ServerBuild,
    string Service);
