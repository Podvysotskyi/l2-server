using L2.Server.Contracts;

namespace L2.Server.Services.Interfaces;

public sealed record CharacterOperationResult(
    bool Succeeded,
    string? ErrorCode = null,
    PlayerCharacterSummary? Character = null);
