namespace L2.Server.Contracts;

public sealed record CharacterOperationResult(
    bool Succeeded,
    string? ErrorCode = null,
    PlayerCharacterSummary? Character = null);
