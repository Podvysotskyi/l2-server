using L2.Server.Contracts;

namespace L2.Server.Repositories.Interfaces;

public sealed record CharacterMutationResult(
    bool Succeeded,
    string? ErrorCode = null,
    PlayerCharacterSummary? Character = null);
